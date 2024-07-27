
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Godot;

public class ProductionModule : LogicModule
{   
    public override void Calculate(List<RegimeTurnOrders> orders,
        LogicWriteKey key)
    {
        var results = key.Data.GetAll<Regime>()
            .AsParallel()
            .Select(r => DoRegime(r, key))
            .ToArray();
        var proc = new ProdResultProcedure(results);
        key.SendMessage(proc);
    }

    private ProductionResult DoRegime(Regime r, LogicWriteKey key)
    {
        var d = key.Data;
        foreach (var f in key.Data.Models.ModelsById.Values.OfType<Flow>())
        {
            r.Stock.Stock.Set(f, 0f);
        }
        var newStock = RegimeStock.Construct();
        newStock.Stock.Add(r.Stock.Stock);
        foreach (var f in key.Data.Models.ModelsById.Values.OfType<Flow>())
        {
            var flowAmt = f.GetNonBuildingSupply(r, d);
            newStock.Stock.Set(f, flowAmt);
            newStock.Produced.Set(f, flowAmt);
        }

        var employments = r.GetPeeps(d)
            .ToDictionary(p => p.Id, 
                p => PeepEmploymentReport.Construct());
        
        var result = new ProductionResult(r.MakeRef(), 
            newStock, new Dictionary<int, int>(), 
            new List<MakeProject>(),
            employments);
        
        DoProd(r, d, result);
        foreach (var (id, employment) in result.Employment)
        {
            var peep = d.Get<Peep>(id);
            var total = employment.Counts.Contents.Values.Sum();
            var unemployed = peep.Size - total;
            employment.Counts.Set(d.Models.PeepJobs.Unemployed, unemployed);
        }
        TroopMaintenance(r, d, result);
        DoMake(r, result, key);
        HandleFoodConsumption(r, newStock, result, d);
        
        return result;
    }

    private static void TroopMaintenance(Regime r, Data d, 
        ProductionResult result)
    {
        var newStock = result.Stock;
        var units = r.GetUnits(d).ToArray();
        var milCap = d.Models.Items.MilitaryCap;
        var milCapCost = 0f;
        foreach (var unit in units)
        {
            foreach (var (troop, amt) in unit.Troops.GetEnumModel(d))
            {
                milCapCost += troop.Makeable.MaintainCosts.Get(milCap) * amt;
            }
        }

        var milCapAvail = newStock.Stock.Get(milCap);
        newStock.RecurringCosts.Add(milCap, milCapCost);
        newStock.Stock.Remove(milCap, Mathf.Min(milCapAvail, milCapCost));
    }


    private class ProdEntry
    {
        public IModel Model { get; private set; }
        public LaborComponent Labor;
        public float Num;
        public float Satisfied;
        public LandCell Cell;

        public ProdEntry(IModel model, LaborComponent labor, float num, LandCell cell)
        {
            Model = model;
            Labor = labor;
            Num = num;
            Cell = cell;
            Satisfied = 0f;
        }
    }
    private static void DoProd(Regime r,
        Data d, ProductionResult result)
    {
        var cells = r
            .GetCells(d).OfType<LandCell>().ToArray();
        var newStock = result.Stock;
        var cellFreeLabor = cells
            .ToDictionary(c => c,
                c => c.GetPeep(d).Size);

        var foodProds = cells.SelectMany(c =>
        {
            return c.FoodProd
                .Nums.GetEnumModel(d)
                .Select(kvp =>
                    new ProdEntry(kvp.Key, kvp.Key.Labor, kvp.Value, c));
        }).ToArray();
        
        var resourceExtractions = cells
            .Select(c =>
            {
                var dep = c.GetResourceDeposit(d);
                if (dep is null) return null;
                if (dep.Extraction.Fulfilled() == false) return null;
                return new ProdEntry(dep.Extraction.Get(d), dep.Extraction.Get(d).Labor, 1f, c);
            })
            .Where(v => v is not null).ToArray();

        var settlementBuildings = cells
            .Where(c => c.HasSettlement(d))
            .SelectMany(c =>
            { 
                if (c.GetSettlement(d) is Settlement s == false) return null;
                return s.Buildings
                    .GetEnumModel(d)
                    .Select(kvp =>
                        new ProdEntry(kvp.Key, kvp.Key.Labor, kvp.Value, c));
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
            
            var laborReq = entry.Labor.Jobs
                .Contents.Sum(kvp => kvp.Value)
                * unsatisfied * num;
            var laborRatio = laborAvail / laborReq;
            if (float.IsNaN(laborRatio)) throw new Exception();
            laborRatio = Mathf.Clamp(laborRatio, 0f, 1f);
            
            var inputRatio = 1f;
            if (entry.Labor.Inputs.Contents.Count > 0)
            {
                inputRatio = entry.Labor.Inputs.Contents
                    .Min(kvp => newStock.Stock.Get(kvp.Key) / (kvp.Value * num * unsatisfied));
                if (float.IsNaN(inputRatio)) throw new Exception();
                inputRatio = Mathf.Clamp(inputRatio, 0f, 1f);
            }

            var ratio = Mathf.Min(laborRatio, inputRatio);
            if (ratio == 0f) continue;

            sinceLast = 0;
            var satisfactionIncrement = ratio * unsatisfied;
            entry.Satisfied += satisfactionIncrement;
            foreach (var (id, amt) in entry.Labor.Inputs.Contents)
            {
                var inputAmt = amt * num * unsatisfied * ratio;
                newStock.Stock.Remove(id, inputAmt);
                newStock.RecurringCosts.Add(id, inputAmt);
            }
            foreach (var (id, amt) in entry.Labor.Outputs.Contents)
            {
                var outputAmt = amt * num * unsatisfied * ratio;
                newStock.Stock.Add(id, outputAmt);
                newStock.Produced.Add(id, outputAmt);
            }

            var employment = result.Employment[entry.Cell.GetPeep(d).Id];
            foreach (var (id, amt) in entry.Labor.Jobs.Contents)
            {
                try
                {
                    var job = d.Models.GetModel<PeepJob>(id);
                    employment.Counts.Add(job, amt * num * ratio * unsatisfied);

                }
                catch (Exception e)
                {
                    GD.Print($"coudlnt find job {id} for {entry.Model.Name}");
                    throw;
                }
            }
        }
    }
    
    private static void HandleFoodConsumption(
        Regime regime,
        RegimeStock newStock,
        ProductionResult result,
        Data d)
    {
        var food = d.Models.Items.Food;
        var foodConsPerPop = d.BaseDomain.Rules.FoodConsumptionPerPeepPoint;
        var pop = regime.GetPopulation(d);
        var foodDemanded = pop * foodConsPerPop;
        var foodStock = Mathf.FloorToInt(newStock.Stock.Get(d.Models.Items.Food));
        var actualCons = Math.Min(foodStock, foodDemanded);
        var surplusRatio = (float) foodStock / foodDemanded - 1f;
        newStock.RecurringCosts.Add(food, actualCons);
        newStock.Stock.Remove(food, actualCons);
        if (surplusRatio > 0f)
        {
            HandleGrowth(regime, surplusRatio, result, d);
        }
        else
        {
            // HandleDecline(regime, -surplusRatio, growthsByPeep, key.Data);
        }
    }
    private static void HandleGrowth(Regime regime, 
        float surplusRatio, ProductionResult result,
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
            result.PeepGrowths.Add(peepsToAffect[i].Id, growthPerPeep);
        }
    }

    private static void DoMake(Regime r, 
            ProductionResult result,
            LogicWriteKey key)
    {
        var d = key.Data;
        var newStock = result.Stock;
        var queue = r.MakeQueue.Queue;
        foreach (var proj in queue)
        {
            var costs = proj.GetMakeable(d).BuildCosts;
            var made = BuildTree.Increment(proj.GetMakeable(d),
                newStock,
                proj.Amount - proj.Fulfilled,
                key);
            
            if (proj.Fulfilled >= proj.Amount)
            {
                proj.Finish(key);
            }
            else
            {
                result.MakeQueue.Add(proj);
            }
        }
    }
    
}
