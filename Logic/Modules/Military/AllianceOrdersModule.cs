
using System;
using System.Collections.Generic;
using System.Linq;

public class AllianceOrdersModule : LogicModule
{
    public override void Calculate(List<RegimeTurnOrders> orders, 
        LogicWriteKey key)
    {
        var allianceOrders = orders
            .Where(o =>
            {
                var r = o.Regime.Entity(key.Data);
                var a = r.GetAlliance(key.Data);
                return r.Id == a.Leader.RefId;
            })
            .Select(o => ((MajorTurnOrders)o).Alliance);
        foreach (var aOrders in allianceOrders)
        {
            foreach (var kvp in aOrders.NewFrontWaypointsByRegimeId)
            {
                var regime = key.Data.Get<Regime>(kvp.Item1);
                var front = Front.Create(regime, kvp.Item2.ToHashSet(), key);
            }
        }
    }
}