using System;
using System.Collections.Generic;
using System.Linq;
using GDMeshSharp3.Exception;
using Godot;

public class FrontAssignment : ForceAssignment
{
    public Front Front { get; private set; }

    public FrontAssignment(Front front) : base()
    {
        Front = front;
    }

    public override void CalculateOrders(MinorTurnOrders orders, 
        LogicWriteKey key)
    {
        if (Groups.Count == 0) return;
        var alliance = orders.Regime.Entity(key.Data).GetAlliance(key.Data);
        var areaRadius = 500f;
        var frontWps = 
            Front.GetContactLineWaypoints(key.Data);
        var avgPos = key.Data.Planet.GetAveragePosition(frontWps.Select(wp => wp.Pos));
        
        
        //todo handle this
        if (frontWps.Count < 2) return;
        var shiftLength = 50f;

        
        var relTo = Front.RelTo(key.Data);
        
        Assigner.AssignAlongLine(
            frontWps,
            Groups.ToList(),
            g => g.GetPowerPoints(key.Data),
            (v,w) => GetDefendCost(v, w, key.Data),
            v => GetPos(v, alliance, key.Data),
            (v1, v2) => key.Data.Planet.GetOffsetTo(v1, v2),
            (g, l) =>
            {
                var order = new DeployOnLineOrder(l);
                var proc = new SetUnitOrderProcedure(g.MakeRef(), order);
                key.SendMessage(proc);
            }
        );
    }

    public float GetDefendCost(Waypoint wp1, Waypoint wp2, Data data)
    {
        var offset = data.Planet.GetOffsetTo(wp1.Pos, wp2.Pos);
        if (offset.Length() == 0f)
        {
            throw new Exception($"0 offset {wp1.Id} {wp1.Pos} to {wp2.Id} {wp2.Pos}");
        }

        return offset.Length();
        var dCost1 = wp1.GetDefendCost(data);
        var dCost2 = wp2.GetDefendCost(data);
        return offset.Length() * (dCost1 + dCost2);
    }

    private Vector2 GetPos(Waypoint wp, Alliance alliance, Data data)
    {
        var shift = Vector2.Zero;
        var shiftLength = 15f;
        var enemyNs = wp.GetNeighboringTacWaypoints(data)
            .Where(n => data.Context
                .WaypointForceBalances[n]
                .Any(kvp => alliance.Rivals.Contains(kvp.Key)));
        if (wp.IsDirectlyThreatened(alliance, data))
        {
            var enemyUnits = data.Military.UnitAux.UnitGrid
                .GetWithin(wp.Pos, 200f);
            foreach (var enemyN in enemyUnits)
            {
                var offset = data.Planet.GetOffsetTo(enemyN.Position, wp.Pos);
                shift += offset.Normalized() * enemyN.GetPowerPoints(data);
            }
        }
        
        return wp.Pos + shift.Normalized() * shiftLength;
    }
    public float GetPowerPointRatio(Data data)
    {
        var powerPoints = GetPowerPointsAssigned(data);
        if (powerPoints == 0f) return 0f;
        var opposing = Front.GetOpposingPowerPoints(data);
        if (opposing == 0f) return Mathf.Inf;
        return powerPoints / opposing;
    }
}