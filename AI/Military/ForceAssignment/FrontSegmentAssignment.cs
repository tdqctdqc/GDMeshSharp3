using System;
using System.Collections.Generic;
using MathNet.Numerics;
using System.Linq;
using Godot;
using MessagePack;

public class FrontSegmentAssignment : ForceAssignment
{
    public Vector2 Center { get; private set; }
    public HashSet<int> TacWaypointIds { get; private set; }

    public static FrontSegmentAssignment Construct(
        EntityRef<Regime> r,
        IEnumerable<Waypoint> heldWaypoints,
        LogicWriteKey key)
    {
        var center = key.Data.Planet.GetAveragePosition(heldWaypoints.Select(wp => wp.Pos));
        
        return new FrontSegmentAssignment(key.Data.IdDispenser.TakeId(),
            heldWaypoints.Select(wp => wp.Id).ToHashSet(),
            center,
            new HashSet<int>(),
            r);
    }
    [SerializationConstructor] private FrontSegmentAssignment(
        int id,
        HashSet<int> tacWaypointIds,
        Vector2 center,
        HashSet<int> groupIds, 
        EntityRef<Regime> regime) 
        : base(groupIds, regime, id)
    {
        TacWaypointIds = tacWaypointIds;
        Center = center;
    }

    
    public override void CalculateOrders(MinorTurnOrders orders, LogicWriteKey key)
    {
        // if (GroupIds.Count == 0) return;
        var areaRadius = 500f;
        if (GroupIds.Count() == 0) return;
        // var list = GetLine(key.Data);
        // Assigner.AssignAlongLine(list.ToList(),
        //     Groups(key.Data).ToList(),
        //     t => t.GetPowerPoints(key.Data),
        //     (v,w) => v.GetOffsetTo(w, key.Data).Length(),
        //     v => v,
        //     (v,w) => v.GetOffsetTo(w, key.Data),
        //     (g, l) =>
        //     {
        //         var order = new DeployOnLineOrder(l);
        //         var proc = new SetUnitOrderProcedure(g.MakeRef(), order);
        //         key.SendMessage(proc);
        //     });
    }
    public IEnumerable<Waypoint> GetTacWaypoints(Data d)
    {
        return TacWaypointIds.Select(id => MilitaryDomain.GetTacWaypoint(id, d));
    }
    public float GetDefenseNeed(Data data, 
        float totalLength, float coverLengthWeight,
        float totalOpposing, float coverOpposingWeight)
    {
        var opposing = GetOpposingPowerPoints(data);
        var length = TacWaypointIds.Count;
    
        var res = 0f;
        if (totalOpposing != 0f)
        {
            res += coverOpposingWeight * opposing / totalOpposing;
        }
    
        if (totalLength != 0f)
        {
            res += coverLengthWeight * length / totalLength;
        }
        return res;
    }
    public float GetOpposingPowerPoints(Data data)
    {
        var forceBalances = data.Context.WaypointForceBalances;
        var alliance = Regime.Entity(data).GetAlliance(data);
        return GetTacWaypoints(data)
            .Sum(wp => forceBalances[wp].GetHostilePowerPoints(alliance, data));
    }
}