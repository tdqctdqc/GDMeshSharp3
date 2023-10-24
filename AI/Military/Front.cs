
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
}