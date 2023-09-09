
using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Collections.Generic;
using System.Diagnostics;
using Godot;
using MessagePack;

public class ProduceConstructProcedure : Procedure
{
    public ConcurrentDictionary<int, ItemCount> RegimeResourceProds { get; private set; }
    public ConcurrentDictionary<int, RegimeFlows> RegimeFlows { get; private set; }
    public ConcurrentDictionary<int, PeepEmploymentReport> EmploymentReports { get; private set; }
    public ConcurrentDictionary<PolyTriPosition, float> ConstructionProgresses { get; private set; }
    public List<KeyValuePair<int, PolymorphMessage<ManufactureProject>>> ManufacturingProjectsToAddByRegime { get; private set; }
    public List<KeyValuePair<int, int>>  ManufacturingProjectsToCancelByRegime { get; private set; }
    
    public static ProduceConstructProcedure Create()
    {
        return new ProduceConstructProcedure(
            new ConcurrentDictionary<int, ItemCount>(), 
            new ConcurrentDictionary<int, PeepEmploymentReport>(),
            new ConcurrentDictionary<PolyTriPosition, float>(),
            new ConcurrentDictionary<int, RegimeFlows>(),
            new List<KeyValuePair<int, PolymorphMessage<ManufactureProject>>>(),
            new List<KeyValuePair<int, int>>());
    }
    [SerializationConstructor] private ProduceConstructProcedure(
        ConcurrentDictionary<int, ItemCount> regimeResourceProds, 
        ConcurrentDictionary<int, PeepEmploymentReport> employmentReports,
        ConcurrentDictionary<PolyTriPosition, float> constructionProgresses,
        ConcurrentDictionary<int, RegimeFlows> regimeFlows,
        List<KeyValuePair<int, PolymorphMessage<ManufactureProject>>> manufacturingProjectsToAddByRegime,
        List<KeyValuePair<int, int>>  manufacturingProjectsToCancelByRegime)
    {
        ConstructionProgresses = constructionProgresses;
        RegimeResourceProds = regimeResourceProds;
        EmploymentReports = employmentReports;
        RegimeFlows = regimeFlows;
        ManufacturingProjectsToAddByRegime = manufacturingProjectsToAddByRegime;
        ManufacturingProjectsToCancelByRegime = manufacturingProjectsToCancelByRegime;
    }

    public override bool Valid(Data data)
    {
        return true;
    }

    public override void Enact(ProcedureWriteKey key)
    {
        var sw = new Stopwatch();
        EnactFlows(key);

        EnactProduce(key);

        EnactConstruct(key);

        EnactManufacture(key);

        foreach (var kvp in EmploymentReports)
        {
            var poly = key.Data.Get<MapPolygon>(kvp.Key);
            poly.GetPeep(key.Data).SetEmploymentReport(kvp.Value, key);
        }
    }

    private void EnactFlows(ProcedureWriteKey key)
    {
        foreach (var kvp in RegimeFlows)
        {
            var r = (Regime)key.Data[kvp.Key];
            var flows = kvp.Value;
            r.SetFlows(flows, key);
        }
    }
    private void EnactProduce(ProcedureWriteKey key)
    {
        var tick = key.Data.Tick;
        foreach (var kvp in RegimeResourceProds)
        {
            var r = (Regime)key.Data[kvp.Key];
            var gains = kvp.Value;
            foreach (var kvp2 in gains.Contents)
            {
                var item = (Item)key.Data.Models[kvp2.Key];
                var gain = kvp2.Value;
                var itemReport = r.History.ItemHistory[item, tick];
                itemReport.Produced += gain;
                r.Items.Add(item, gain);
            }
        }
    }
    
    private void EnactConstruct(ProcedureWriteKey key)
    {
        foreach (var kvp in ConstructionProgresses)
        {
            var pos = kvp.Key;
            var r = pos.Poly(key.Data).Regime.Entity(key.Data);
            var construction = key.Data.Infrastructure.CurrentConstruction.ByTri[pos];
            construction.ProgressConstruction(kvp.Value,  key);
        }
    }

    private void EnactManufacture(ProcedureWriteKey key)
    {
        foreach (var kvp in ManufacturingProjectsToAddByRegime)
        {
            var regime = key.Data.Get<Regime>(kvp.Key);
            var member = PolymorphMember<ManufactureProject>.Construct(kvp.Value.Get(key.Data));
            regime.ManufacturingQueue.Queue.Enqueue(member);
        }

        foreach (var kvp in ManufacturingProjectsToCancelByRegime)
        {
            //todo implement 
            // var regime = key.Data.Get<Regime>(kvp.Key);
            // var toCancel = kvp.Value;
            // regime.ManufacturingQueue.Queue.
        }

        foreach (var regime in key.Data.GetAll<Regime>())
        {
            var ip = RegimeFlows[regime.Id][key.Data.Models.Flows.IndustrialPower].Net();
            regime.ManufacturingQueue.Manufacture(ip, regime, key);
        }
    }
}
