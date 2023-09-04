
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
    private ConcurrentDictionary<int, PeepEmploymentReport> 
        _polyEmployReps = new ConcurrentDictionary<int, PeepEmploymentReport>();
    private ConcurrentDictionary<int, PolyEmploymentScratch>
        _polyScratches = new ConcurrentDictionary<int, PolyEmploymentScratch>();

    private void Clear()
    {
        foreach (var kvp in _regimeProdWallets) { kvp.Value.Clear(); }
        foreach (var kvp in _polyEmployReps) { kvp.Value.Counts.Clear(); }
    }
    public override LogicResults Calculate(List<TurnOrders> orders, Data data)
    {
        Clear();
        var res = new LogicResults();
        var tick = data.BaseDomain.GameClock.Tick;
        var proc = ProduceConstructProcedure.Create();
        
        Parallel.ForEach(data.GetAll<Regime>(), 
            regime => CalculateForRegime(regime, data, proc));
        DoManufacturing(orders, proc);
        res.Messages.Add(proc);
        return res;
    }

    private void CalculateForRegime(Regime regime, Data data, ProduceConstructProcedure proc)
    {
        DoNonBuildingFlows(regime, data, proc);

        var gains = _regimeProdWallets.GetOrAdd(regime.Id,
            id => ItemCount.Construct());
        proc.RegimeResourceProds.TryAdd(regime.Id, gains);
        var unemployedJob = data.Models.PeepJobs.Unemployed;
        
        var regimePolys = regime.GetPolys(data);
        var totalLaborerUnemployed = 0;
        
        foreach (var poly in regimePolys)
        {
            var scratch = _polyScratches.GetOrAdd(poly.Id,
                p => new PolyEmploymentScratch((MapPolygon) data[p], data));
            scratch.Init(poly, data);
            ProduceFoodForPoly(poly, proc, scratch, data);
            WorkInBuildingsForPoly(poly, proc, scratch, data);
        }
        
        ConstructForRegime(regime, data, proc);
        
        foreach (var poly in regimePolys)
        {
            var scratch = _polyScratches[poly.Id];
            var numUnemployed = scratch.Available;
            var employment = _polyEmployReps.GetOrAdd(poly.Id, p => PeepEmploymentReport.Construct());
            employment.Clear();
            employment.Counts.AddOrSum(unemployedJob.Id, numUnemployed);
            proc.EmploymentReports[poly.Id] = employment;
            foreach (var kvp in scratch.ByJob)
            {
                employment.Counts[kvp.Key.Id] = kvp.Value;
            }
        }
        
    }

    private void ConstructForRegime(Regime regime, Data data, ProduceConstructProcedure proc)
    {
        var regimePolys = regime.GetPolys(data);
        var construction = data.Infrastructure.CurrentConstruction.ByPoly;

        var constrCap = data.Models.Flows.ConstructionCap;
        var constrFlowIn = constrCap.GetNonBuildingSupply(regime, data);
        var constrFlowOut = constrCap.GetConsumption(regime, data);
        
        if (constrFlowOut == 0) return;
        var constructRatio = Mathf.Clamp((float)constrFlowIn / (float)constrFlowOut, 0f, 1f);
        foreach (var poly in regimePolys)
        {
            ConstructForPoly(regime, poly, constructRatio, proc, data);
        }
    }

    private void ProduceFoodForPoly(MapPolygon poly, ProduceConstructProcedure proc, PolyEmploymentScratch scratch, Data data)
    {
        var peep = poly.GetPeep(data);
        if (peep == null) return;
        var foodProd = scratch.HandleFoodProdJobs(poly.PolyFoodProd, data);
        proc.RegimeResourceProds[poly.Regime.RefId].Add(data.Models.Items.Food, foodProd);
    }
    private void WorkInBuildingsForPoly(MapPolygon poly, ProduceConstructProcedure proc, PolyEmploymentScratch scratch, Data data)
    {
        var peep = poly.GetPeep(data);
        if (peep == null) return;
        IEnumerable<MapBuilding> mapBuildings = poly.GetBuildings(data);
        if(mapBuildings == null) return;
        var workBuildings = mapBuildings.Where(b => b.Model.Model(data).HasComponent<Workplace>());
        if (workBuildings.Count() == 0) return;
        var workplaces = workBuildings.Select(b => b.Model.Model(data).GetComponent<Workplace>());
        var effectiveRatio = scratch.HandleBuildingJobs(workplaces, data);
        var laborNeed = workBuildings.Sum(b => b.Model.Model(data).GetComponent<Workplace>().TotalLaborReq());
        foreach (var b in mapBuildings)
        {
            foreach (var c in b.Model.Model(data).Components)
            {
                c.Work(proc, poly, effectiveRatio, data);
            }
        }
    }

    private void ConstructForPoly(Regime r, MapPolygon poly,
        float ratio, ProduceConstructProcedure proc, Data data)
    {
        IEnumerable<Construction> constructions = data.Infrastructure.CurrentConstruction.GetPolyConstructions(poly);
        if (constructions == null || constructions.Count() == 0) return;
        foreach (var construction in constructions)
        {
            var spend = ratio * construction.Model.Model(data).ConstructionCapPerTick;
            proc.ConstructionProgresses.TryAdd(construction.Pos, ratio);
        }
    }

    private void DoManufacturing(List<TurnOrders> orders, ProduceConstructProcedure proc)
    {
        foreach (var turnOrders in orders)
        {
            if (turnOrders is MajorTurnOrders m == false) throw new Exception();
            var regime = turnOrders.Regime.RefId;
            foreach (var polymorphMessage in m.ManufacturingOrders.ToStart)
            {
                proc.ManufacturingProjectsToAddByRegime
                    .Add(new(regime, polymorphMessage));
            }
            
            foreach (var toCancel in m.ManufacturingOrders.ToCancel)
            {
                proc.ManufacturingProjectsToCancelByRegime
                    .Add(new KeyValuePair<int, int>(regime, toCancel));
            }
        }
    }
    private void DoNonBuildingFlows(Regime r, Data data, ProduceConstructProcedure proc)
    {
        foreach (var kvp in data.Models.GetModels<Flow>())
        {
            var flow = kvp.Value;
            var amt = flow.GetNonBuildingSupply(r, data);
            var flows = proc.RegimeFlows.GetOrAdd(r.Id, i => new RegimeFlows(new Dictionary<int, FlowData>()));
            var consumption = flow.GetConsumption(r, data);
            proc.RegimeFlows[r.Id].AddFlowIn(flow, amt);
            proc.RegimeFlows[r.Id].AddFlowOut(flow, consumption);
        }
    }
}
