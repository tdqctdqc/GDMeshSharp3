
using System.Collections.Generic;
using System.Linq;
using Godot;
using MessagePack;

public class Front : Entity
{
    public EntityRef<Regime> Regime { get; private set; }
    public HashSet<int> WaypointIds { get; private set; }
    public static Front Create(Regime r, IEnumerable<int> frontline,
        ICreateWriteKey key)
    {
        var f = new Front(r.MakeRef(), frontline.ToHashSet(), key.Data.IdDispenser.TakeId());
        key.Create(f);
        return f;
    }
    
    [SerializationConstructor] private Front(EntityRef<Regime> regime,
        HashSet<int> waypointIds, int id)
        : base(id)
    {
        Regime = regime;
        WaypointIds = waypointIds;
    }

    public Vector2 RelTo(Data d)
    {
        var p = d.Planet.Nav.Waypoints[WaypointIds.First()].Pos;
        return d.Planet.ClampPosition(p);
    }

    public IEnumerable<Waypoint> GetWaypoints(Data data)
    {
        return WaypointIds.Select(i => data.Planet.Nav.Waypoints[i]);
    }

    public List<List<Waypoint>> GetFrontlines(Data data)
    {
        var context = data.Context;
        var alliance = Regime.Entity(data).GetAlliance(data);
        
        bool isThreatened(Waypoint wp)
        {
            var controlling = context
                .WaypointForceBalances[wp]
                .GetControllingAlliances();
            return controlling
                .Any(a => alliance.Rivals.Contains(a));
        }
        
        var frontWps = GetWaypoints(data);

        var directlyThreatened = frontWps
            .Where(isThreatened).ToHashSet();
        var indirectlyThreatened = frontWps
            .Except(directlyThreatened)
            .Where(wp =>
            {
                return wp.GetNeighboringWaypoints(data)
                    .Any(nWp => isThreatened(nWp) && directlyThreatened.Contains(nWp) == false);
            });
        var frontlineWps =
            directlyThreatened
                // .Union(indirectlyThreatened)
                .Distinct()
                .ToList();
        
        if (frontlineWps.Count == 1)
            return new List<List<Waypoint>> { new List<Waypoint> { frontlineWps.First() } };
        var frontSegs = new List<LineSegment>();
        foreach (var wp in frontlineWps)
        {
            foreach (var nWp in wp.GetNeighboringWaypoints(data))
            {
                if (frontlineWps.Contains(nWp) == false) continue;
                if (nWp.Id < wp.Id) continue;
                frontSegs.Add(new LineSegment(wp.Pos, nWp.Pos));
            }
        }

        var chains = LineSegmentExt.GetChains(frontSegs);

        return chains
            .Select(c => c.GetPoints())
            .Select(ps => ps.Select(p => data.Planet.Nav.WaypointsByPos[p]).ToList())
            .ToList();
    }
}