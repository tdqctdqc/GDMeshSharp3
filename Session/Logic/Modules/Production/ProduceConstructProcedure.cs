
using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Collections.Generic;
using System.Diagnostics;
using Godot;
using MessagePack;

public class ProduceConstructProcedure : Procedure
{
    public ConcurrentDictionary<int, ItemCount> RegimeResourceGains { get; private set; }
    public ConcurrentDictionary<int, FlowCount> RegimeInflows { get; private set; }
    public ConcurrentDictionary<int, EmploymentReport> EmploymentReports { get; private set; }
    public ConcurrentDictionary<PolyTriPosition, float> ConstructionProgresses { get; private set; }

    public static ProduceConstructProcedure Create()
    {
        return new ProduceConstructProcedure(
            new ConcurrentDictionary<int, ItemCount>(), 
            new ConcurrentDictionary<int, EmploymentReport>(),
            new ConcurrentDictionary<PolyTriPosition, float>(),
            new ConcurrentDictionary<int, FlowCount>());
    }
    [SerializationConstructor] private ProduceConstructProcedure(
        ConcurrentDictionary<int, ItemCount> regimeResourceGains, 
        ConcurrentDictionary<int, EmploymentReport> employmentReports,
        ConcurrentDictionary<PolyTriPosition, float> constructionProgresses,
        ConcurrentDictionary<int, FlowCount> regimeInflows)
    {
        ConstructionProgresses = constructionProgresses;
        RegimeResourceGains = regimeResourceGains;
        EmploymentReports = employmentReports;
        RegimeInflows = regimeInflows;
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
        
        
        foreach (var kvp in EmploymentReports)
        {
            var poly = key.Data.Planet.Polygons[kvp.Key];
            poly.SetEmploymentReport(kvp.Value, key);
        }
    }

    private void EnactFlows(ProcedureWriteKey key)
    {
        foreach (var kvp in RegimeInflows)
        {
            var r = (Regime)key.Data[kvp.Key];
            var flows = kvp.Value;
            r.SetFlows(flows, key);
        }
    }
    private void EnactProduce(ProcedureWriteKey key)
    {
        var tick = key.Data.Tick;
        foreach (var kvp in RegimeResourceGains)
        {
            var r = (Regime)key.Data[kvp.Key];
            var gains = kvp.Value;
            foreach (var kvp2 in gains.Contents)
            {
                var item = (Item)key.Data.Models[kvp2.Key];
                r.Items.Add(item, kvp2.Value);
            }
            r.History.ProdHistory.TakeSnapshot(tick, gains);
        }
    }
    

    private void EnactConstruct(ProcedureWriteKey key)
    {
        foreach (var kvp in ConstructionProgresses)
        {
            var pos = kvp.Key;
            var r = pos.Poly(key.Data).Regime.Entity();
            var construction = key.Data.Society.CurrentConstruction.ByTri[pos];
            construction.ProgressConstruction(kvp.Value,  key);
        }
    }
}
