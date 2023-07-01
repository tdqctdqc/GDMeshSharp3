
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Godot;

public class WorkProdConsumeModule : LogicModule
{
    private int _lastRunTick;
    private int _ticksSinceLast;
    private ConcurrentDictionary<int, ItemWallet> 
        _regimeProdWallets = new ConcurrentDictionary<int, ItemWallet>();
    private ConcurrentDictionary<int, ItemWallet> 
        _regimeConsWallets = new ConcurrentDictionary<int, ItemWallet>();
    private ConcurrentDictionary<int, ItemWallet> 
        _regimeDemandWallets = new ConcurrentDictionary<int, ItemWallet>();
    private ConcurrentDictionary<int, EmploymentReport> 
        _polyEmployReps = new ConcurrentDictionary<int, EmploymentReport>();

    private ConcurrentDictionary<int, PolyEmploymentScratch>
        _polyScratches = new ConcurrentDictionary<int, PolyEmploymentScratch>();

    private void Clear()
    {
        foreach (var kvp in _regimeProdWallets) { kvp.Value.Clear(); }
        foreach (var kvp in _regimeConsWallets) { kvp.Value.Clear(); }
        foreach (var kvp in _regimeDemandWallets) { kvp.Value.Clear(); }
        foreach (var kvp in _polyEmployReps) { kvp.Value.Counts.Clear(); }
    }
    public override LogicResults Calculate(Data data)
    {
        var res = new LogicResults();
        var tick = data.BaseDomain.GameClock.Tick;
        _ticksSinceLast = tick - _lastRunTick;
        _lastRunTick = tick;
        var proc = WorkProdConsumeProcedure.Create(_ticksSinceLast);
        
        Parallel.ForEach(data.Society.Regimes.Entities, 
            regime => WorkForRegime(regime, data, proc));
        
        Parallel.ForEach(data.Society.Regimes.Entities,
            regime => ConsumeForRegime(proc, regime, data));
        
        res.Procedures.Add(proc);
        return res;
    }

    private void WorkForRegime(Regime regime, Data data, WorkProdConsumeProcedure proc)
    {
        var gains = _regimeProdWallets.GetOrAdd(regime.Id,
            id => ItemWallet.Construct());
        proc.RegimeResourceGains.TryAdd(regime.Id, gains);
        var unemployedJob = PeepJobManager.Unemployed;
        
        var regimePolys = regime.Polygons;
        var totalLaborerUnemployed = 0;
        
        foreach (var poly in regimePolys)
        {
            var scratch = _polyScratches.GetOrAdd(poly.Id,
                p => new PolyEmploymentScratch((MapPolygon) data[p], data));
            scratch.Init(poly, data);
            ProduceFoodForPoly(poly, proc, scratch, data);
            WorkInBuildingsForPoly(poly, proc, scratch, data);
            totalLaborerUnemployed += scratch.Available;
        }
        
        ConstructForRegime(regime, data, proc, totalLaborerUnemployed);
        
        foreach (var poly in regimePolys)
        {
            var scratch = _polyScratches[poly.Id];
            var numUnemployed = scratch.Available;
            var employment = _polyEmployReps.GetOrAdd(poly.Id, p => EmploymentReport.Construct());
            employment.Clear();
            employment.Counts.AddOrSum(unemployedJob.Id, numUnemployed);
            proc.EmploymentReports[poly.Id] = employment;
            foreach (var kvp in scratch.ByJob)
            {
                // if(kvp.Value.Total > 0) GD.Print(poly.Id + " Writing " + kvp.Value.Total);
                employment.Counts[kvp.Key.Id] = kvp.Value;
            }
        }
    }

    private void ConstructForRegime(Regime regime, Data data, WorkProdConsumeProcedure proc,
        int totalLaborerUnemployed)
    {
        var builderJob = PeepJobManager.Builder;
        var regimePolys = regime.Polygons;

        var construction = data.Society.CurrentConstruction.ByPoly;

        var constructionLaborNeeded = regime.Polygons
            .Where(p => construction.ContainsKey(p.Id))
            .Select(p => construction[p.Id].Sum(c => c.Model.Model().LaborPerTickToBuild))
            .Sum();
        var constructLaborRunningTotal = constructionLaborNeeded;
        if (constructionLaborNeeded == 0) return;
        
        var constructLaborRatio = 0f;
        
        if (totalLaborerUnemployed == 0) constructLaborRatio = 0f;
        else constructLaborRatio = Mathf.Clamp((float)totalLaborerUnemployed / (float)constructionLaborNeeded, 0f, 1f);
        foreach (var poly in regimePolys)
        {
            ConstructForPoly(poly, constructLaborRatio, proc, data);
        }
        
        // foreach (var poly in regimePolys)
        // {
        //     var scratch = _polyScratches[poly.Id];
        //     
        //     var constructLabor = scratch.HandleConstructionJobs(data, totalLaborerUnemployed,
        //         constructionLaborNeeded, constructLaborRunningTotal);
        //     constructLaborRunningTotal -= constructLabor;
        //     ConstructForPoly(poly, scratch, constructLaborRatio, proc, data);
        // }
        //
        // var totalConstructLabor = regimePolys.Sum(p =>
        // {
        //     var scratch = _polyScratches[p.Id].ByJob;
        //     if (scratch.ContainsKey(builderJob))
        //     {
        //         return scratch[builderJob];
        //     }
        //
        //     return 0;
        // });
        // if (totalConstructLabor > constructionLaborNeeded)
        // {
        //     throw new Exception($"needed {constructionLaborNeeded} have {totalConstructLabor}");
        // }
        //
        // var effective = Mathf.FloorToInt(constructionLaborNeeded * constructLaborRatio);
        // if (totalConstructLabor < effective)
        // {
        //     throw new Exception($"expected {effective} have {totalConstructLabor} ratio {constructLaborRatio}");
        // }

    }

    private void ProduceFoodForPoly(MapPolygon poly, WorkProdConsumeProcedure proc, PolyEmploymentScratch scratch, Data data)
    {
        var peep = poly.GetPeep(data);
        if (peep == null) return;
        var foodProd = scratch.HandleFoodProdJobs(poly.PolyFoodProd, data);
        proc.RegimeResourceGains[poly.Regime.RefId].Add(ItemManager.Food, foodProd);
    }
    private void WorkInBuildingsForPoly(MapPolygon poly, WorkProdConsumeProcedure proc, PolyEmploymentScratch scratch, Data data)
    {
        var peep = poly.GetPeep(data);
        if (peep == null) return;
        IEnumerable<MapBuilding> mapBuildings = poly.GetBuildings(data);
            if(mapBuildings == null) return;
            
        mapBuildings = mapBuildings.Where(b => b.Model.Model() is WorkBuildingModel);
        if (mapBuildings.Count() == 0) return;
        
        IEnumerable<WorkBuildingModel> workBuildings = poly.GetBuildings(data)
            .Where(b => b.Model.Model() is WorkBuildingModel)
            .Select(b => (WorkBuildingModel)b.Model.Model());
        if (workBuildings.Count() == 0) return;
        var effectiveRatio = scratch.HandleBuildingJobs(workBuildings, data);
        foreach (var wb in workBuildings)
        {
            wb.Produce(proc, poly, effectiveRatio, _ticksSinceLast, data);
        }
    }

    private void ConstructForPoly(MapPolygon poly,
        float ratio, WorkProdConsumeProcedure proc, Data data)
    {
        IEnumerable<Construction> constructions = data.Society.CurrentConstruction.GetPolyConstructions(poly);
        if (constructions == null || constructions.Count() == 0) return;
        foreach (var construction in constructions)
        {
            proc.ConstructionProgresses.TryAdd(construction.Pos, ratio);
        }
    }

    private void ConsumeForRegime(WorkProdConsumeProcedure proc, Regime regime, Data data)
    {
        var consumptions = _regimeConsWallets.GetOrAdd(regime.Id,
            id => ItemWallet.Construct());
        proc.ConsumptionsByRegime.TryAdd(regime.Id, consumptions);

        var demands = _regimeDemandWallets.GetOrAdd(regime.Id,
            id => ItemWallet.Construct());
        proc.DemandsByRegime.TryAdd(regime.Id, demands);
        
        var numPeeps = regime.Polygons
            .Where(p => p.HasPeep(data))
            .Select(p => p.GetPeep(data))
            .Sum(p => p.Size);
        var foodDesired = numPeeps * data.BaseDomain.Rules.FoodConsumptionPerPeepPoint * _ticksSinceLast;
        demands.Add(ItemManager.Food, foodDesired);
        var foodStock = regime.Items[ItemManager.Food] 
                        // + proc.RegimeResourceGains[regime.Id][ItemManager.Food]
                        ;
        var foodConsumption = Mathf.Min(foodDesired, foodStock);
        consumptions.Add(ItemManager.Food, foodConsumption);
    }
}
