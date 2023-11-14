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
        
        Assigner.AssignAlongLine2(
            frontWps,
            Groups.ToList(),
            g => g.GetPowerPoints(key.Data),
            (wp1, wp2) =>
            {
                var offset = key.Data.Planet.GetOffsetTo(wp1.Pos, wp2.Pos);
                if (offset.Length() == 0f)
                {
                    
                    throw new Exception($"0 offset {wp1.Id} {wp1.Pos} to {wp2.Id} {wp2.Pos}");
                }
                var dCost1 = wp1.GetDefendCost(key.Data);
                if (dCost1 == 0f) throw new Exception();
                var dCost2 = wp2.GetDefendCost(key.Data);
                if (dCost2 == 0f) throw new Exception();
                return Mathf.Max(.1f, offset.Length()) * (dCost1 + dCost2);
            },
            wp =>
            {
                var shift = Vector2.Zero;

                var enemyNs = wp.GetNeighboringWaypoints(key.Data)
                    .Where(n => key.Data.Context
                        .WaypointForceBalances[n]
                        .Any(kvp => alliance.Rivals.Contains(kvp.Key)));
                foreach (var enemyN in enemyNs)
                {
                    var offset = key.Data.Planet.GetOffsetTo(enemyN.Pos, wp.Pos);

                    shift += offset.Normalized()
                             * enemyN.GetForceBalance(key.Data)
                                 .Where(kvp => alliance.IsHostileTo(kvp.Key))
                                 .Sum(kvp => kvp.Value);
                    
                }
                return wp.Pos + shift.Normalized() * shiftLength;
            },
            (v1, v2) => key.Data.Planet.GetOffsetTo(v1, v2),
            (g, l) =>
            {
                var order = new DeployOnLineOrder(l);
                var proc = new SetUnitOrderProcedure(g.MakeRef(), order);
                key.SendMessage(proc);
            }
        );
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