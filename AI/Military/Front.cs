
using System;
using System.Collections.Generic;
using System.Linq;
using Godot;
using MessagePack;

public class Front
{
    public int Id { get; private set; }
    public EntityRef<Regime> Regime { get; private set; }
    public List<int> ContactLineWaypointIds { get; private set; }
    public static Front Construct(Regime r, List<int> waypointIds, ICreateWriteKey key)
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
        var p = d.Military.TacticalWaypoints.Waypoints[ContactLineWaypointIds.First()].Pos;
        return d.Planet.ClampPosition(p);
    }

    public List<Waypoint> GetContactLineWaypoints(Data data)
    {
        return ContactLineWaypointIds.Select(i => data.Military.TacticalWaypoints.Waypoints[i]).ToList();
    }

    public float GetOpposingPowerPoints(Data data)
    {
        var forceBalances = data.Context.WaypointForceBalances;
        var alliance = Regime.Entity(data).GetAlliance(data);
        return GetContactLineWaypoints(data)
            .Sum(wp => forceBalances[wp].GetHostilePowerPoints(alliance, data));
    }

    public float GetLength(Data data)
    {
        var res = 0f;
        for (var i = 0; i < ContactLineWaypointIds.Count - 1; i++)
        {
            var wp1 = data.Military.TacticalWaypoints.Waypoints[ContactLineWaypointIds[i]];
            var wp2 = data.Military.TacticalWaypoints.Waypoints[ContactLineWaypointIds[i + 1]];
            res += data.Planet.GetOffsetTo(wp1.Pos, wp2.Pos).Length();
        }

        return res;
    }
}