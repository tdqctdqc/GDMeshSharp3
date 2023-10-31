
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
        return d.Planet.Nav.Waypoints[WaypointIds.First()].Pos;
    }

    public IEnumerable<Waypoint> GetWaypoints(Data data)
    {
        return WaypointIds.Select(i => data.Planet.Nav.Waypoints[i]);
    }

    public List<Waypoint> GetFrontline(Data data)
    {
        var context = data.Context;
        var alliance = Regime.Entity(data).GetAlliance(data);
        
        bool frontline(Waypoint wp)
        {
            var ns = wp
                .GetNeighboringWaypoints(data);
            return ns
                .Any(nWp => 
                    context.WaypointForceBalances[nWp]
                    .GetControllingAlliances()
                    .Any(a => alliance.Rivals.Contains(a))
                );
        }
        
        var frontlineWps = GetWaypoints(data)
            .Where(frontline);
        //todo have to order these properly!
        return frontlineWps.ToList();
    }
}