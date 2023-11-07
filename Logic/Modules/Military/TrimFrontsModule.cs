
using System;
using System.Collections.Generic;
using System.Linq;

public class TrimFrontsModule : LogicModule
{
    public TrimFrontsModule()
    {
    }
    public override void Calculate(List<RegimeTurnOrders> orders, 
        LogicWriteKey key)
    {
        var trim = TrimFrontsProcedure.Construct();

        foreach (var alliance in key.Data.GetAll<Alliance>())
        {
            TrimFronts(alliance, trim, key.Data, key);
        }
        key.SendMessage(trim);
    }
    public void TrimFronts(Alliance alliance, TrimFrontsProcedure proc,
        Data data, LogicWriteKey key)
    {
    }

    private void CheckFront(Front front, TrimFrontsProcedure proc,
        HashSet<int> controlled, Data data, LogicWriteKey key)
    {
        var regime = front.Regime.Entity(data);
        var toRemove = front.ContactLineWaypointIds
            .Where(i => controlled.Contains(i) == false);
        if(toRemove.Count() == 0) return;
        if (toRemove.Count() == front.ContactLineWaypointIds.Count())
        {
            proc.FrontsToRemove.Add(front.Id);
            return;
        }

        var toStay = front.ContactLineWaypointIds.Except(toRemove);
        var floodFill = FloodFill<int>.GetFloodFill(
            toStay.First(), toStay.Contains,
            i => data.Planet.Nav.Waypoints[i].Neighbors);
        if (floodFill.Count == toStay.Count())
        {
            proc.WaypointsToTrimByFrontId.Add(front.Id, toRemove.ToHashSet());
            return;
        }

        proc.FrontsToRemove.Add(front.Id);
        var unions = UnionFind.Find(
            toStay, (i, j) => true,
            i => data.Planet.Nav.Waypoints[i].Neighbors);

        foreach (var union in unions)
        {
            var newFront = Front.Construct(regime, union, key);
        }
    }
}