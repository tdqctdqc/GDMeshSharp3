using System;
using System.Collections.Generic;
using System.Linq;
using GDMeshSharp3.Exception;
using Godot;
using MathNet.Numerics.Statistics;

public class FrontAssignment : ForceAssignment
{
    public Front Front { get; private set; }
    public List<FrontSegmentAssignment> Segments { get; private set; }

    public FrontAssignment(Front front, HashSet<int> groupIds,
        List<FrontSegmentAssignment> segments) 
        : base(groupIds, front.Regime)
    {
        Segments = segments;
        Front = front;
    }
    
    public override void CalculateOrders(MinorTurnOrders orders, 
        LogicWriteKey key)
    {
        var alliance = orders.Regime.Entity(key.Data).GetAlliance(key.Data);
        var areaRadius = 500f;
        var relTo = Front.RelTo(key.Data);

        var frontWps = 
            Front.GetHeldWaypoints(key.Data);
        if (frontWps.Count() < 2) return;
        CalcSegments(key.Data);
        
        FrontSegmentAssignment.CalcPositions(this, key.Data);
        if (Segments.Any(fa => fa.Left == fa.Right))
        {
            GD.Print("bad");
        }
        
        foreach (var fsa in Segments)
        {
            fsa.CalculateOrders(orders, key);
        }
        
        // var defendCostByFrom = lines.ToDictionary(kvp => kvp.Key.From,
        //     kvp => kvp.Value);
        //
        // Assigner.AssignAlongLine<Vector2, int>(
        //     lines.Keys.Select(ls => ls.From).ToList(),
        //     GroupIds.ToList(),
        //     g => key.Data.Get<UnitGroup>(g).GetPowerPoints(key.Data),
        //     (v,w) => defendCostByFrom[v],
        //     wp => wp,
        //     (v1, v2) => key.Data.Planet.GetOffsetTo(v1, v2),
        //     (g, l) =>
        //     {
        //         var order = new DeployOnLineOrder(l);
        //         var proc = new SetUnitOrderProcedure(new EntityRef<UnitGroup>(g), order);
        //         key.SendMessage(proc);
        //     }
        // );
    }

    public void CalcSegments(Data d)
    {
        Segments.Clear();
        var maxSegLength = 100f;
        var frontSegmentWpCount = 10;
        var alliance = Front.Regime.Entity(d).GetAlliance(d);
        var relTo = Front.RelTo(d);
        var toPick = new HashSet<Waypoint>(Front.GetHeldWaypoints(d));
        var frontier = new HashSet<Waypoint>();
        var segmentWps = new HashSet<Waypoint>();
        
        while (toPick.Count > 0)
        {
            frontier.Clear();
            segmentWps.Clear();
            
            var seed = toPick.First();
            var curr = seed;
            while (curr != null && segmentWps.Count < frontSegmentWpCount)
            {
                toPick.Remove(curr);
                segmentWps.Add(curr);
                frontier.Remove(curr);
                frontier.AddRange(curr.TacNeighbors(d).Where(toPick.Contains));

                if (frontier.Count > 0)
                {
                    curr = frontier
                        .OrderBy(f => seed.Pos.GetOffsetTo(f.Pos, d).Length())
                        .First();
                }
                else
                {
                    curr = null;
                }
            }

            var seg = FrontSegmentAssignment.Construct(Regime, segmentWps, d);
            Segments.Add(seg);
        }
        
        //handle leftovers

        var undersized = Segments
            .Where(s => s.HeldWaypointIds.Count < frontSegmentWpCount)
            .ToList();
        
        
        foreach (var u in undersized)
        {
            if (u.HeldWaypointIds.Count > frontSegmentWpCount) continue;
            var wps = u.GetHeldWaypoints(d);
            var neighbors = getNeighboring();

            if (neighbors.Count() > 0)
            {
                var first = neighbors
                    .MinBy(n => n.HeldWaypointIds.Count);
                first.HeldWaypointIds.AddRange(u.HeldWaypointIds);
                u.HeldWaypointIds.Clear();
                Segments.Remove(u);
            }
            
            
            IEnumerable<FrontSegmentAssignment> getNeighboring()
            {
                return Segments.Where(s => 
                    s != u && s.HeldWaypointIds.Count > 0
                    && isNeighbor(s));
            }

            bool isNeighbor(FrontSegmentAssignment s)
            {
                return wps.Any(wp => s.GetHeldWaypoints(d).Any(swp => swp.Neighbors.Contains(wp.Id)));
            }
        }
        
        float getCost(Waypoint wp)
        {
            var fb = d.Context.WaypointForceBalances[wp];
            return fb.GetHostilePowerPoints(alliance, d)
                   + fb.GetHostilePowerPointsOfNeighbors(wp, alliance, d);
        }
    }

    public float GetDefendCost(Waypoint wp1, Waypoint wp2, Data data)
    {
        var offset = wp1.Pos.GetOffsetTo(wp2.Pos, data);
        if (offset.Length() == 0f)
        {
            throw new Exception($"0 offset {wp1.Id} {wp1.Pos} to {wp2.Id} {wp2.Pos}");
        }

        return offset.Length();
        var dCost1 = wp1.GetDefendCost(data);
        var dCost2 = wp2.GetDefendCost(data);
        return offset.Length() * (dCost1 + dCost2);
    }

    
    // private float GetFrontDefenseNeed(Data data, 
    //     float totalLength, float coverLengthWeight,
    //     float totalOpposing, float coverOpposingWeight)
    // {
    //     var opposing = Front.GetOpposingPowerPoints(data);
    //     var length = Front.GetLength(data);
    //
    //     var res = 0f;
    //     if (totalOpposing != 0f)
    //     {
    //         res += coverOpposingWeight * opposing / totalOpposing;
    //     }
    //
    //     if (totalLength != 0f)
    //     {
    //         res += coverLengthWeight * length / totalLength;
    //     }
    //     return res;
    // }
}