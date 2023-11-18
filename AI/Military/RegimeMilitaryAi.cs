
using System.Collections.Generic;
using System.Linq;

public class RegimeMilitaryAi
{
    private Regime _regime;
    public ForceCompositionAi ForceComposition { get; private set; }
    public DeploymentAi Deployment { get; private set; }

    public RegimeMilitaryAi(Regime regime)
    {
        _regime = regime;
        ForceComposition = new ForceCompositionAi(regime);
        Deployment = new DeploymentAi();
    }

    
    public void CalculateMajor(LogicWriteKey key, MajorTurnOrders orders)
    {
        var reserve = IdCount<Troop>.Construct(_regime.Military.TroopReserve);
        ForceComposition.Calculate(_regime, key, orders, reserve);
    }

    public void CalculateMinor(LogicWriteKey key, MinorTurnOrders orders)
    {
        Deployment.Calculate(_regime, key, orders);
    }
    
    public static List<List<Waypoint>> GetContactLines(Regime regime, IEnumerable<Waypoint> wps, 
        Data data)
    {
        var context = data.Context;
        var alliance = regime.GetAlliance(data);
        
        bool isThreatened(Waypoint wp)
        {
            var controlling = context
                .WaypointForceBalances[wp]
                .GetControllingAlliances();
            return controlling
                .Any(a => alliance.Rivals.Contains(a));
        }

        var directlyThreatened = wps
            .Where(isThreatened).ToHashSet();
        
        var threatenedWps =
            directlyThreatened
                .Distinct()
                .ToList();
        
        if (threatenedWps.Count == 1)
            return new List<List<Waypoint>> { new List<Waypoint> { threatenedWps.First() } };
        var frontSegs = new List<LineSegment>();
        foreach (var wp in threatenedWps)
        {
            foreach (var nWp in wp.GetNeighboringTacWaypoints(data))
            {
                if (threatenedWps.Contains(nWp) == false) continue;
                if (nWp.Id < wp.Id) continue;
                frontSegs.Add(new LineSegment(wp.Pos, nWp.Pos));
            }
        }

        var chains = LineSegmentExt.GetChains(frontSegs);

        return chains
            .Select(c => c.GetPoints())
            .Select(ps => ps.Select(p => data.Military.TacticalWaypoints.ByPos[p])
                .Select(i => data.Military.TacticalWaypoints.Waypoints[i])
                .ToList())
            .ToList();
    }
}