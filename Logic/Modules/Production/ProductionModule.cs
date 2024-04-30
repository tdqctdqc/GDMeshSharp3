
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Godot;

public class ProductionModule : LogicModule
{
    public override void Calculate(List<RegimeTurnOrders> orders, LogicWriteKey key)
    {
        var results = key.Data.GetAll<Regime>()
            .AsParallel()
            .Select(r => DoRegime(r, key.Data))
            .ToArray();
        var proc = new ProdResultProcedure(results);
        key.SendMessage(proc);
    }

    private ProductionResult DoRegime(Regime r, Data d)
    {
        var newStock = RegimeStock.Construct();
        foreach (var (id, amt) in r.Stock.Stock.Contents.ToList())
        {
            var model = d.Models.GetModel<IModel>(id);
            if (model is Flow)
            {
                r.Stock.Stock.Set(model, 0f);
            }
        }
        var cells = d.Planet.MapAux.CellHolder
            .Cells.Values.Where(c => c.Controller.RefId == r.Id).ToArray();
        
        var totalPop = cells
            .Sum(c => c.GetPeep(d).Size);
        var labor = d.Models.Flows.Labor;
        r.Stock.Stock.Set(labor, totalPop);
        r.Stock.Produced.Set(labor, totalPop);
        
        var growths = DoFood(r, newStock, d);
        DoBuildingProds(r, d, newStock);
        TroopMaintenance(r, d, newStock);
        
        var constructCap = d.Models.Flows.ConstructionCap;
        var constructCapProduced = r.GetPopulation(d);
        constructCapProduced = Mathf.FloorToInt(constructCapProduced);
        if (constructCapProduced < 0f) throw new Exception();
        r.Stock.Stock.Set(constructCap, constructCapProduced);
        newStock.Produced.Set(constructCap, constructCapProduced);
        
        var made = DoMake(r, newStock, d);
        
        newStock.Stock.Add(r.Stock.Stock);
        
        var result = new ProductionResult(r.MakeRef(), 
            newStock, growths, made);
        return result;
    }

    private static void TroopMaintenance(Regime r, Data d, RegimeStock res)
    {
        var units = r.GetUnits(d);
        var milCap = d.Models.Flows.MilitaryCap;
        var milCapCost = 0f;
        foreach (var unit in units)
        {
            foreach (var (troop, amt) in unit.Troops.GetEnumerableModel(d))
            {
                milCapCost += troop.Makeable.MaintainCosts.Get(milCap) * amt;
            }
        }

        var milCapAvail = r.Stock.Stock.Get(milCap);
        res.RecurringCosts.Add(milCap, milCapCost);
        r.Stock.Stock.Remove(milCap, Mathf.Min(milCapAvail, milCapCost));
    }

    private static void DoBuildingProds(Regime r, Data d,
        RegimeStock res)
    {
        var prodBuildings = r.GetCells(d)
            .Where(c => c.HasSettlement(d))
            .SelectMany(c => c.GetSettlement(d)
                .Buildings.GetEnumerableModel(d))
            .Where(kvp => kvp.Key.HasComponent<BuildingProd>())
            .ConsolidateCounts();

        var buildingProdQueue = new PriorityQueue<
            (SettlementBuildingModel, float num, float satisfied), float>();
        foreach (var (b, num) in prodBuildings)
        {
            buildingProdQueue.Enqueue((b, num, 0f), 1f);
        }

        int itersSinceLastProd = 0;
        while (buildingProdQueue.TryDequeue(out var e,
                   out var p))
        {
            var (model, num, satisfied) = e;
            var prod = model.GetComponent<BuildingProd>();
            var satisfactionIncrement = prod.Inputs.GetEnumerableModel(d)
                .Min(kvp => r.Stock.Stock.Get(kvp.Key) / (kvp.Value * num));
            satisfactionIncrement = Mathf.Clamp(satisfactionIncrement, 0f, 1f - satisfied);

            if (satisfactionIncrement > 0f)
            {
                itersSinceLastProd = 0;
                foreach (var (inputModel, inputAmt)
                         in prod.Inputs.GetEnumerableModel(d))
                {
                    r.Stock.Stock.Remove(inputModel,
                        inputAmt * num * satisfactionIncrement);
                    res.RecurringCosts.Add(inputModel,
                        inputAmt * num * satisfactionIncrement);
                }

                foreach (var (outputModel, outputAmt)
                         in prod.Outputs.GetEnumerableModel(d))
                {
                    r.Stock.Stock.Add(outputModel,
                        outputAmt * num * satisfactionIncrement);
                    res.Produced.Add(outputModel,
                        outputAmt * num * satisfactionIncrement);
                }
            }
            else
            {
                itersSinceLastProd++;
                if (itersSinceLastProd > buildingProdQueue.Count)
                {
                    break;
                }
            }

            satisfied += satisfactionIncrement;
            if (satisfied < 1f)
            {
                buildingProdQueue.Enqueue((model, num, satisfied), p - 1f);
            }
        }
    }

    private static Dictionary<int, int> DoFood(
        Regime regime,
        RegimeStock res,
        Data d)
    {
        var food = d.Models.Items.Food;
        var growthsByPeep = new Dictionary<int, int>();
        var foodConsPerPop = d.BaseDomain.Rules.FoodConsumptionPerPeepPoint;
        var pop = regime.GetPopulation(d);
        
        var maxSurplusRatio = d.BaseDomain.Rules.MaxEffectiveSurplusRatio;
        var foodDemanded = foodConsPerPop * pop * (1f + maxSurplusRatio);
        var foodProds = regime.GetCells(d)
            .OfType<LandCell>()
            .Select(c => c.FoodProd.Nums)
            .MergeCounts()
            .OrderByDescending(kvp => kvp.Key.Get(d).FoodPerLabor());
        var count = foodProds.Count();
        
        var totalProduced = 0f;
        int iter = 0;
        while (totalProduced < foodDemanded && iter < count)
        {
            var demand = foodDemanded - totalProduced;
            var kvp = foodProds.ElementAt(iter);

            iter++;
            var prodModel = kvp.Key.Get(d);
            var amt = kvp.Value;
            var possibleProd = prodModel.BaseProd * amt;
            var laborAvail = res.Stock.Get(d.Models.Flows.Labor);
            
            var prodRatio = demand / possibleProd;
            prodRatio = Mathf.Clamp(prodRatio, 0f, 1f);

            var laborNeeded = prodRatio * amt * prodModel.BaseLabor;
            var laborRatio = laborAvail / laborNeeded;
            laborRatio = Mathf.Clamp(laborRatio, 0f, 1f);

            prodRatio = Mathf.Clamp(prodRatio, 0f, laborRatio);
            
            var produced = prodRatio * possibleProd;
            totalProduced += produced;
            var labor = prodRatio * amt * prodModel.BaseLabor;
            
            regime.Stock.Stock.Add(d.Models.Items.Food, produced);
            res.Produced.Add(d.Models.Items.Food, produced);
            res.RecurringCosts.Add(d.Models.Flows.Labor, labor);
            regime.Stock.Stock.Remove(d.Models.Flows.Labor, labor);
        }
        
        var foodStock = Mathf.FloorToInt(regime.Stock.Stock.Get(d.Models.Items.Food));
        var actualCons = Math.Min(foodStock, foodDemanded);
        var surplusRatio = (float) foodStock / foodDemanded - 1f;
        res.RecurringCosts.Add(food, actualCons);
        if (surplusRatio > 0f)
        {
            HandleGrowth(regime, surplusRatio, growthsByPeep, d);
        }
        else
        {
            // HandleDecline(regime, -surplusRatio, growthsByPeep, key.Data);
        }

        return growthsByPeep;
    }
    private static void HandleGrowth(Regime regime, 
        float surplusRatio, Dictionary<int, int> growths,
        Data data)
    {
        var rules = data.BaseDomain.Rules;
        if (rules.MinSurplusRatioToGetGrowth > surplusRatio) return;
        
        var range = rules.MaxEffectiveSurplusRatio - rules.MinSurplusRatioToGetGrowth;
        if (range < 0) throw new Exception();
        
        var effectiveRatio = Mathf.Min(surplusRatio / range, rules.MaxEffectiveSurplusRatio);
        if (range < 0) throw new Exception();
        
        var peeps = regime.GetCells(data).Where(p => p.HasPeep(data))
            .Select(p => p.GetPeep(data));
        var numPeeps = peeps.Count();
        if (numPeeps == 0) return;
        
        var effect = rules.GrowthRateCeiling * effectiveRatio * peeps.Sum(p => p.Size);
        if (effect < 0) throw new Exception();

        var numPeepsToAffect = Mathf.CeilToInt(numPeeps / 10f);
        if (numPeepsToAffect < 0) throw new Exception();
        
        var peepsToAffect = peeps.GetDistinctRandomElements(numPeepsToAffect);

        var growthPerPeep = Mathf.CeilToInt(effect / numPeepsToAffect);
        if (growthPerPeep < 0) throw new Exception();
        for (var i = 0; i < peepsToAffect.Count; i++)
        {
            growths.Add(peepsToAffect[i].Id, growthPerPeep);
        }
    }

    private static Dictionary<ModelRef<IModel>, float> 
        DoMake(Regime r, RegimeStock res, Data d)
    {
        var queue = r.MakeQueue.Queue;
        var made = new Dictionary<ModelRef<IModel>, float>();
        foreach (var proj in queue)
        {
            var model = proj.Making.Get(d);
            var costs = ((IMakeable)model).Makeable.BuildCosts;
            var num = proj.Amount;
            var satisfactionIncrement = costs.GetEnumerableModel(d)
                .Min(kvp => r.Stock.Stock.Get(kvp.Key) / (kvp.Value * num));
            satisfactionIncrement = Mathf.Clamp(satisfactionIncrement, 0f, 1f);
            
            if (satisfactionIncrement > 0f)
            {
                foreach (var (inputModel, inputAmt)
                         in costs.GetEnumerableModel(d))
                {
                    r.Stock.Stock.Remove(inputModel,
                        inputAmt * num * satisfactionIncrement);
                    res.SingleTimeCosts.Add(inputModel,
                        inputAmt * num * satisfactionIncrement);
                }

                var amtMade = num * satisfactionIncrement;
                r.Stock.Stock.Add(model, amtMade);
                res.Produced.Add(model, amtMade);
                made.AddOrSum(model.MakeRef(), amtMade);
            }
        }

        return made;
    }
}
