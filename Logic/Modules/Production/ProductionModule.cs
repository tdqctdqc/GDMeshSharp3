
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
        DoProd(r, d, newStock);
        var growths = HandleFoodConsumption(r, newStock, d);
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

    private static void TroopMaintenance(Regime r, Data d, 
        RegimeStock res)
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


    private class ProdEntry
    {
        public ProdComponent Prod;
        public float Num;
        public float Satisfied;
        public LandCell Cell;

        public ProdEntry(ProdComponent prod, float num, LandCell cell)
        {
            Prod = prod;
            Num = num;
            Cell = cell;
            Satisfied = 0f;
        }
    }
    private static void DoProd(Regime r,
        Data d, RegimeStock stock)
    {
        var cells = r
            .GetCells(d).OfType<LandCell>().ToArray();
        foreach (var cell in cells)
        {
            stock.EmploymentReports.Add(cell.Id, PeepEmploymentReport.Construct());
        }
        var cellFreeLabor = cells
            .ToDictionary(c => c,
                c => c.GetPeep(d).Size);

        var foodProds = cells.SelectMany(c =>
        {
            return c.FoodProd
                .Nums.GetEnumerableModel(d)
                .Select(kvp =>
                    new ProdEntry(kvp.Key.Prod, kvp.Value, c));
        }).ToArray();
        
        var resourceExtractions = cells
            .Select(c =>
            {
                var dep = c.GetResourceDeposit(d);
                if (dep is null) return null;
                if (dep.Extraction.Fulfilled() == false) return null;
                return new ProdEntry(dep.Extraction.Get(d).Prod, 1f, c);
            })
            .Where(v => v is not null).ToArray();

        var settlementBuildings = cells
            .Where(c => c.HasSettlement(d))
            .SelectMany(c =>
            { 
                if (c.GetSettlement(d) is Settlement s == false) return null;
                return s.Buildings
                    .GetEnumerableModel(d)
                    .Where(kvp => kvp.Key.HasComponent<ProdComponent>())
                    .Select(kvp =>
                        new ProdEntry(kvp.Key.GetComponent<ProdComponent>(), kvp.Value, c));
            })
            .Where(v => v is not null).ToArray();
        var allProds = foodProds
            .Concat(resourceExtractions).Concat(settlementBuildings)
            .ToArray();


        var iter = 0;
        var sinceLast = 0;
        while (sinceLast < allProds.Length)
        {
            var entry = allProds[iter % allProds.Length];
            iter++;
            sinceLast++;
            if (entry.Satisfied >= 1f) continue;
            var laborAvail = cellFreeLabor[entry.Cell]; 
            if (laborAvail <= 0f) continue;
            var unsatisfied = 1f - entry.Satisfied;

            var num = entry.Num;
            
            var laborReq = entry.Prod.Jobs
                .Contents.Sum(kvp => kvp.Value)
                * unsatisfied * num;
            var laborRatio = laborAvail / laborReq;
            if (float.IsNaN(laborRatio)) throw new Exception();
            laborRatio = Mathf.Clamp(laborRatio, 0f, 1f);
            
            var inputRatio = 1f;
            if (entry.Prod.Inputs.Contents.Count > 0)
            {
                inputRatio = entry.Prod.Inputs.Contents
                    .Min(kvp => stock.Stock.Get(kvp.Key) / (kvp.Value * num * unsatisfied));
                if (float.IsNaN(inputRatio)) throw new Exception();
                inputRatio = Mathf.Clamp(inputRatio, 0f, 1f);
            }

            var ratio = Mathf.Min(laborRatio, inputRatio);
            if (ratio == 0f) continue;

            sinceLast = 0;
            var satisfactionIncrement = ratio * unsatisfied;
            entry.Satisfied += satisfactionIncrement;
            foreach (var (id, amt) in entry.Prod.Inputs.Contents)
            {
                var inputAmt = amt * num * unsatisfied * ratio;
                stock.Stock.Remove(id, inputAmt);
                stock.RecurringCosts.Add(id, inputAmt);
            }
            foreach (var (id, amt) in entry.Prod.Outputs.Contents)
            {
                var outputAmt = amt * num * unsatisfied * ratio;
                stock.Stock.Add(id, outputAmt);
                stock.Produced.Add(id, outputAmt);
            }

            var employment = stock.EmploymentReports[entry.Cell.Id];
            foreach (var (id, amt) in entry.Prod.Jobs.Contents)
            {
                employment.Counts.AddOrSum(id, amt * num * ratio * unsatisfied);
            }
        }
    }
    
    
    
    
    private static Dictionary<int, int> HandleFoodConsumption(
        Regime regime,
        RegimeStock res,
        Data d)
    {
        var food = d.Models.Items.Food;
        var growthsByPeep = new Dictionary<int, int>();
        var foodConsPerPop = d.BaseDomain.Rules.FoodConsumptionPerPeepPoint;
        var pop = regime.GetPopulation(d);
        var foodDemanded = pop * foodConsPerPop;
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
