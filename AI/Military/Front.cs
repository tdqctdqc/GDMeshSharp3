
using System.Collections.Generic;
using System.Linq;
using Godot;
using MessagePack;

public class Front
{
    public int Id { get; private set; }
    public EntityRef<Regime> Regime { get; private set; }
    public List<int> ContactLineWaypointIds { get; private set; }
    public static Front Construct(Regime r, IEnumerable<int> waypointIds, ICreateWriteKey key)
    {
        var f = new Front(r.MakeRef(), waypointIds.ToList(), key.Data.IdDispenser.TakeId());
        return f;
    }
    
    [SerializationConstructor] private Front(EntityRef<Regime> regime,
        List<int> contactLineWaypointIds, int id)
    {
        Id = id;
        Regime = regime;
        ContactLineWaypointIds = contactLineWaypointIds;
    }

    public Vector2 RelTo(Data d)
    {
        var p = d.Planet.Nav.Waypoints[ContactLineWaypointIds.First()].Pos;
        return d.Planet.ClampPosition(p);
    }

    public IEnumerable<Waypoint> GetWaypoints(Data data)
    {
        return ContactLineWaypointIds.Select(i => data.Planet.Nav.Waypoints[i]);
    }

    public float GetOpposingPowerPoints(Data data)
    {
        var forceBalances = data.Context.WaypointForceBalances;
        var alliance = Regime.Entity(data).GetAlliance(data);
        return GetWaypoints(data)
            .Sum(wp => forceBalances[wp].GetHostilePowerPoints(alliance, data));
    }
}