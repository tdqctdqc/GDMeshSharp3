
using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Collections.Generic;
using System.Diagnostics;
using Godot;
using MessagePack;

public class WorkProdConsumeProcedure : Procedure
{
    public int TicksSinceLast { get; private set; }
    public ConcurrentDictionary<int, ItemWallet> RegimeResourceGains { get; private set; }
    public ConcurrentDictionary<int, ItemWallet> ConsumptionsByRegime { get; private set; }
    public ConcurrentDictionary<int, ItemWallet> DemandsByRegime { get; private set; }
    public ConcurrentDictionary<int, EmploymentReport> EmploymentReports { get; private set; }
    public ConcurrentDictionary<PolyTriPosition, float> ConstructionProgresses { get; private set; }
    

    public static WorkProdConsumeProcedure Create(int ticksSinceLast)
    {
        return new WorkProdConsumeProcedure(ticksSinceLast, 
            new ConcurrentDictionary<int, ItemWallet>(), 
            new ConcurrentDictionary<int, ItemWallet>(), new ConcurrentDictionary<int, ItemWallet>(),
            new ConcurrentDictionary<int, EmploymentReport>(),
            new ConcurrentDictionary<PolyTriPosition, float>());
    }
    [SerializationConstructor] private WorkProdConsumeProcedure(
        int ticksSinceLast,
        ConcurrentDictionary<int, ItemWallet> regimeResourceGains, 
        ConcurrentDictionary<int, ItemWallet> consumptionsByRegime,
        ConcurrentDictionary<int, ItemWallet> demandsByRegime,
        ConcurrentDictionary<int, EmploymentReport> employmentReports,
        ConcurrentDictionary<PolyTriPosition, float> constructionProgresses)
    {
        TicksSinceLast = ticksSinceLast;
        ConstructionProgresses = constructionProgresses;
        RegimeResourceGains = regimeResourceGains;
        ConsumptionsByRegime = consumptionsByRegime;
        DemandsByRegime = demandsByRegime;
        EmploymentReports = employmentReports;
    }

    public override bool Valid(Data data)
    {
        return true;
    }

    public override void Enact(ProcedureWriteKey key)
    {
        var sw = new Stopwatch();
        
        EnactProduce(key);
        
        EnactConsume(key);

        EnactConstruct(key);
        
        
        foreach (var kvp in EmploymentReports)
        {
            var poly = key.Data.Planet.Polygons[kvp.Key];
            poly.SetEmploymentReport(kvp.Value, key);
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
    private void EnactConsume(ProcedureWriteKey key)
    {
        var tick = key.Data.BaseDomain.GameClock.Tick;
        foreach (var kvp in ConsumptionsByRegime)
        {
            var r = (Regime)key.Data[kvp.Key];
            var consumptions = kvp.Value;
            var snapshot = kvp.Value.GetSnapshot();

            foreach (var kvp2 in consumptions.Contents)
            {
                var item = (Item)key.Data.Models[kvp2.Key];
                try
                {
                    r.Items.Remove(item, kvp2.Value);
                }
                catch (Exception e)
                {
                    GD.Print($"trying to remove ${kvp2.Value} of ${r.Items[item]} ${item.Name}");
                    throw;
                }
            }
            r.History.ConsumptionHistory.TakeSnapshot(tick, consumptions);
        }
        foreach (var kvp in DemandsByRegime)
        {
            var r = (Regime)key.Data[kvp.Key];
            var demands = kvp.Value;     
            r.History.DemandHistory.TakeSnapshot(tick, demands);
        }
    }

    private void EnactConstruct(ProcedureWriteKey key)
    {
        foreach (var kvp in ConstructionProgresses)
        {
            var pos = kvp.Key;
            var r = pos.Poly(key.Data).Regime.Entity();
            var construction = key.Data.Society.CurrentConstruction.ByTri[pos];
            construction.ProgressConstruction(kvp.Value, TicksSinceLast, key);
        }
    }
}
