
using System;
using System.Collections.Generic;
using System.Linq;
using Godot;
using MessagePack;

public class Front
{
    public int Id { get; private set; }
    public EntityRef<Regime> Regime { get; private set; }
    public HashSet<int> HeldWaypointIds { get; private set; }
    public static Front Construct(Regime r, IEnumerable<int> waypointIds, ICreateWriteKey key)
    {
        var f = new Front(r.MakeRef(), waypointIds.ToHashSet(), key.Data.IdDispenser.TakeId());
        return f;
    }
    
    [SerializationConstructor] private Front(EntityRef<Regime> regime,
        HashSet<int> heldWaypointIds, int id)
    {
        Id = id;
        Regime = regime;
        HeldWaypointIds = heldWaypointIds;
    }

    public Vector2 RelTo(Data d)
    {
        var p = MilitaryDomain.GetTacWaypoint(HeldWaypointIds.First(), d).Pos;
        return p.ClampPosition(d);
    }

    public IEnumerable<Waypoint> GetHeldWaypoints(Data data)
    {
        return HeldWaypointIds.Select(i => MilitaryDomain.GetTacWaypoint(i, data));
    }

    public float GetOpposingPowerPoints(Data data)
    {
        var forceBalances = data.Context.WaypointForceBalances;
        var alliance = Regime.Entity(data).GetAlliance(data);
        return GetHeldWaypoints(data)
            .Sum(wp => forceBalances[wp].GetHostilePowerPoints(alliance, data));
    }
}