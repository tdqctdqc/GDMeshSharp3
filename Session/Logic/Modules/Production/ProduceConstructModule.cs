
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Godot;

public class ProduceConstructModule : LogicModule
{
    private ConcurrentDictionary<int, ItemCount> 
        _regimeProdWallets = new ConcurrentDictionary<int, ItemCount>();
    private ConcurrentDictionary<int, EmploymentReport> 
        _polyEmployReps = new ConcurrentDictionary<int, EmploymentReport>();
    private ConcurrentDictionary<int, PolyEmploymentScratch>
        _polyScratches = new ConcurrentDictionary<int, PolyEmploymentScratch>();

    private void Clear()
    {
        foreach (var kvp in _regimeProdWallets) { kvp.Value.Clear(); }
        foreach (var kvp in _polyEmployReps) { kvp.Value.Counts.Clear(); }
    }
    public override LogicResults Calculate(List<TurnOrders> orders, Data data)
    {
        var res = new LogicResults();
        var tick = data.BaseDomain.GameClock.Tick;
        var proc = ProduceConstructProcedure.Create();
        
        Parallel.ForEach(data.Society.Regimes.Entities, 
            regime => CalculateForRegime(regime, data, proc));
        
        res.Procedures.Add(proc);
        return res;
    }

    private void CalculateForRegime(Regime regime, Data data, ProduceConstructProcedure proc)
    {
        var gains = _regimeProdWallets.GetOrAdd(regime.Id,
            id => ItemCount.Construct());
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
        DoNonBuildingFlows(regime, data, proc);
    }

    private void ConstructForRegime(Regime regime, Data data, ProduceConstructProcedure proc,
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
    }

    private void ProduceFoodForPoly(MapPolygon poly, ProduceConstructProcedure proc, PolyEmploymentScratch scratch, Data data)
    {
        var peep = poly.GetPeep(data);
        if (peep == null) return;
        var foodProd = scratch.HandleFoodProdJobs(poly.PolyFoodProd, data);
        proc.RegimeResourceGains[poly.Regime.RefId].Add(ItemManager.Food, foodProd);
    }
    private void WorkInBuildingsForPoly(MapPolygon poly, ProduceConstructProcedure proc, PolyEmploymentScratch scratch, Data data)
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
            wb.Work(proc, poly, effectiveRatio, data);
        }
    }

    private void ConstructForPoly(MapPolygon poly,
        float ratio, ProduceConstructProcedure proc, Data data)
    {
        IEnumerable<Construction> constructions = data.Society.CurrentConstruction.GetPolyConstructions(poly);
        if (constructions == null || constructions.Count() == 0) return;
        foreach (var construction in constructions)
        {
            proc.ConstructionProgresses.TryAdd(construction.Pos, ratio);
        }
    }

    private void DoNonBuildingFlows(Regime r, Data data, ProduceConstructProcedure proc)
    {
        foreach (var kvp in data.Models.Flows.Models)
        {
            var flow = kvp.Value;
            var amt = flow.GetNonBuildingFlow(r, data);
            proc.RegimeInflows[r.Id].Add(flow, amt);
        }
    }
}
