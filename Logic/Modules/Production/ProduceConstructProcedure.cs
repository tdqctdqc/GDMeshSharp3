
using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Collections.Generic;
using System.Diagnostics;
using Godot;
using MessagePack;

public class ProduceConstructProcedure : Procedure
{
    public ConcurrentDictionary<int, IdCount<Item>> RegimeResourceProds { get; private set; }
    public ConcurrentDictionary<int, RegimeFlows> RegimeFlows { get; private set; }
    public ConcurrentDictionary<int, PeepEmploymentReport> EmploymentReports { get; private set; }
    public ConcurrentDictionary<PolyTriPosition, float> ConstructionProgresses { get; private set; }
    
    public static ProduceConstructProcedure Create()
    {
        return new ProduceConstructProcedure(
            new ConcurrentDictionary<int, IdCount<Item>>(), 
            new ConcurrentDictionary<int, PeepEmploymentReport>(),
            new ConcurrentDictionary<PolyTriPosition, float>(),
            new ConcurrentDictionary<int, RegimeFlows>());
    }
    [SerializationConstructor] private ProduceConstructProcedure(
        ConcurrentDictionary<int, IdCount<Item>> regimeResourceProds, 
        ConcurrentDictionary<int, PeepEmploymentReport> employmentReports,
        ConcurrentDictionary<PolyTriPosition, float> constructionProgresses,
        ConcurrentDictionary<int, RegimeFlows> regimeFlows)
    {
        ConstructionProgresses = constructionProgresses;
        RegimeResourceProds = regimeResourceProds;
        EmploymentReports = employmentReports;
        RegimeFlows = regimeFlows;
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

        ProgressConstruction(key);

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
                var itemReport = r.History.ItemHistory.Get(item, tick);
                itemReport.Produced += gain;
                r.Items.Add(item, gain);
            }
        }
    }
    
    private void ProgressConstruction(ProcedureWriteKey key)
    {
        foreach (var kvp in ConstructionProgresses)
        {
            var pos = kvp.Key;
            var r = pos.Poly(key.Data).OwnerRegime.Entity(key.Data);
            var construction = key.Data.Infrastructure.CurrentConstruction.ByTri[pos];
            construction.ProgressConstruction(kvp.Value,  key);
        }
    }

    private void EnactManufacture(ProcedureWriteKey key)
    {
        foreach (var regime in key.Data.GetAll<Regime>())
        {
            var ip = RegimeFlows[regime.Id].Get(key.Data.Models.Flows.IndustrialPower).Net();
            regime.ManufacturingQueue.Manufacture(ip, regime, key);
        }
    }
}
