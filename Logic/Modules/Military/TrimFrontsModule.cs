
using System.Collections.Generic;
using System.Linq;

public class TrimFrontsModule : LogicModule
{
    public TrimFrontsModule()
    {
    }
    public override LogicResults Calculate(List<RegimeTurnOrders> orders, 
        Data data)
    {
        var res = new LogicResults();
        var key = new LogicWriteKey(data, res);
        var trim = TrimFrontsProcedure.Construct();
        res.Messages.Add(trim);
        return res;
    }
    public void TrimFronts(Alliance alliance, TrimFrontsProcedure proc,
        Data data, LogicWriteKey key)
    {
        var controlled = data.Context
            .ControlledAreas[alliance].Select(wp => wp.Id)
            .ToHashSet();
        var regimes = alliance.Members.Items(data);
        
        foreach (var regime in regimes)
        {
            foreach (var front in regime.Military.Fronts.Items(data))
            {
                CheckFront(front, proc, controlled, data, key);
            }
        }
    }

    private void CheckFront(Front front, TrimFrontsProcedure proc,
        HashSet<int> controlled, Data data, LogicWriteKey key)
    {
        var regime = front.Regime.Entity(data);
        var toRemove = front.WaypointIds
            .Where(i => controlled.Contains(i) == false);
        if(toRemove.Count() == 0) return;
        if (toRemove.Count() == front.WaypointIds.Count())
        {
            proc.FrontsToRemove.Add(front.Id);
            return;
        }

        var toStay = front.WaypointIds.Except(toRemove);
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
            var newFront = Front.Create(regime, union, key);
        }
    }
}