using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text.RegularExpressions;
using GDMeshSharp3.Exception;
using Godot;
using MathNet.Numerics.Statistics;
using Microsoft.FSharp.Data.UnitSystems.SI.UnitNames;

public class FrontAssignment : ForceAssignment, ICompoundForceAssignment
{
    
    public static float CoverOpposingWeight = 2f;
    public static float CoverLengthWeight = 1f;
    public static float DesiredOpposingPpRatio = 2f;
    public static float PowerPointsPerCellFaceToCover = 10f;
    public HashSet<int> HeldCellIds { get; private set; }
    public HashSet<int> TargetAreaCellIds { get; private set; }
    public HashSet<ForceAssignment> Assignments { get; private set; }
    public Color Color { get; private set; }
    public FrontAssignment(
        int id,
        EntityRef<Regime> regime, 
        HashSet<int> heldCellIds,
        HashSet<int> targetAreaCellIds,
        HashSet<int> groupIds,
        HashSet<ForceAssignment> assignments,
        Color color) 
        : base(groupIds, regime, id)
    {
        HeldCellIds = heldCellIds;
        TargetAreaCellIds = targetAreaCellIds;
        Assignments = assignments;
        Color = color;
    }

    
    public float GetOpposingPowerPoints(Data data)
    {
        var alliance = Regime.Entity(data).GetAlliance(data);
        return GetCells(data)
            .SelectMany(c => c.GetNeighbors(data))
            .Distinct()
            .Where(n => n.RivalControlled(alliance, data))
            .Sum(n =>
            {
                var us = n.GetUnits(data);
                if(us == null) return 0f;
                return us.Sum(u => u.GetPowerPoints(data));
            });
    }
    public override void CalculateOrders(MinorTurnOrders orders, 
        LogicWriteKey key)
    {
        foreach (var assgn in Assignments)
        {
            assgn.CalculateOrders(orders, key);
        }
    }
    

    public override void TakeAwayGroup(UnitGroup g, LogicWriteKey key)
    {
        this.TakeAwayGroupCompound(g, key);
    }

    public override void AssignGroups(LogicWriteKey key)
    {
        this.AssignFreeGroups(key);
        this.ShiftGroups(key);
        foreach (var assgn in Assignments)
        {
            assgn.AssignGroups(key);
        }
    }
    
    
    public override float GetPowerPointNeed(Data data)
    {
        var opposing = GetOpposingPowerPoints(data);
        var length = Assignments.OfType<FrontSegmentAssignment>().Sum(s => s.GetLength(data));
        return opposing * CoverOpposingWeight + length * CoverLengthWeight * PowerPointsPerCellFaceToCover;
    }
    public List<List<(PolyCell native, PolyCell foreign)>> GetLines(Data d)
    {
        var alliance = Regime.Entity(d).GetAlliance(d);
        var cells = GetCells(d);
        var lines = FrontFinder.FindFrontNew<PolyCell>(cells,
            c =>
            {
                if (c.Controller.Empty()) return false;
                var controllerRegime = c.Controller.Entity(d);
                var controllerAlliance = controllerRegime.GetAlliance(d);
                return alliance.Rivals.Contains(controllerAlliance);
            },
            c => HeldCellIds.Contains(c.Id),
            c => c.GetNeighbors(d),
            (p,q) => p.GetCenter().GetOffsetTo(q.GetCenter(), d));
        if (lines.Any(l => l.Count == 0))
        {
            throw new Exception();
        }

        var res = new List<List<(PolyCell native, PolyCell foreign)>>();
        foreach (var l in lines)
        {
            if (l.Count <= FrontSegmentAssignment.IdealSegmentLength * 2)
            {
                res.Add(l);
                continue;
            }

            var facesPerSeg = Mathf.CeilToInt((float)l.Count / FrontSegmentAssignment.IdealSegmentLength);
            var startI = 0;
            while (startI < l.Count)
            {
                var count = Mathf.Min(facesPerSeg, l.Count - startI);
                res.Add(l.GetRange(startI, count));
                startI += count;
            }
        }
        return res;
        bool sharesEnd(LinkedList<(PolyCell native, PolyCell foreign)> ll,
            List<(PolyCell native, PolyCell foreign)> cand)
        {
            var lineFirst = ll.First.Value.native;
            var lineLast = ll.Last.Value.native;
            var candFirst = cand.First().native;
            var candLast = cand.Last().native;
            return linked(lineFirst, candFirst) 
                   || linked(lineFirst, candLast) 
                   || linked(lineLast, candFirst) 
                   || linked(lineLast, candLast);
        }

        void merge(LinkedList<(PolyCell native, PolyCell foreign)> ll,
            List<(PolyCell native, PolyCell foreign)> shares)
        {
            var lineFirst = ll.First.Value.native;
            var lineLast = ll.Last.Value.native;
            var sharesFirst = shares.First().native;
            var sharesLast = shares.Last().native;
            if (linked(lineFirst, sharesFirst))
            {
                for (var i = 0; i < shares.Count; i++)
                {
                    ll.AddFirst(shares[i]);
                }
            }
            else if (linked(lineFirst, sharesLast))
            {
                for (var i = shares.Count - 1; i >= 0; i--)
                {
                    ll.AddFirst(shares[i]);
                }
            }
            else if (linked(lineLast, sharesFirst))
            {
                for (var i = 0; i < shares.Count; i++)
                {
                    ll.AddLast(shares[i]);
                }
            }
            else if (linked(lineLast, sharesLast))
            {
                for (var i = shares.Count - 1; i >= 0; i--)
                {
                    ll.AddLast(shares[i]);
                }
            }
        }

        bool linked(PolyCell p1, PolyCell p2)
        {
            return p1 == p2 || p1.Neighbors.Contains(p2.Id);
        }
    }

    
    public void MergeInto(FrontAssignment dissolve, LogicWriteKey key)
    {
        GroupIds.AddRange(dissolve.GroupIds);
        Assignments.AddRange(dissolve.Assignments);
    }


    public void CheckSegments(LogicWriteKey key)
    {
        var d = key.Data;
        var lines = GetLines(d)
            .ToDictionary(l => l, 
                l => new List<FrontSegmentAssignment>());
        
        var segments = Assignments.OfType<FrontSegmentAssignment>().ToList();
        foreach (var fsa in segments)
        {
            var relatedLines = lines.Keys
                .Where(l => l.Any(wp => fsa.FrontLineFaces.Contains((wp.native.Id, wp.foreign.Id))));
            if (relatedLines.Count() == 0)
            {
                Assignments.Remove(fsa);
                continue; 
            }
            var mostRelated = relatedLines
                .MaxBy(l => l.Where(wp => fsa.FrontLineFaces.Contains((wp.native.Id, wp.foreign.Id))).Count());
            lines[mostRelated].Add(fsa);
        }
        
        foreach (var kvp in lines)
        {
            var line = kvp.Key;
            var segs = kvp.Value;
            if (segs.Count() == 0)
            {
                var seg = FrontSegmentAssignment
                    .Construct(Regime, line, 
                        false, key);
                Assignments.Add(seg);
                continue;
            }

            var biggest = segs.MaxBy(s => s.GroupIds.Count());
            
            biggest.FrontLineFaces.Clear();
            biggest.FrontLineFaces.AddRange(line.Select(wp => (wp.native.Id, wp.foreign.Id)));
            foreach (var fsa in segs)
            {
                if (fsa == biggest) continue;
                biggest.MergeInto(fsa, key);
                Assignments.Remove(fsa);
            }
        }
    }

    public override PolyCell GetCharacteristicCell(Data d)
    {
        return HeldCellIds.Select(i => PlanetDomainExt.GetPolyCell(i, d))
            .FirstOrDefault(wp => wp.Controller.RefId == Regime.RefId);
    }

    public override UnitGroup RequestGroup(LogicWriteKey key)
    {
        if (GroupIds.Count < 2) return null;
        UnitGroup deassign = null;
        if (Assignments.Count > 0)
        {
            deassign = Assignments
                .MaxBy(s => s.GetSatisfiedRatio(key.Data))
                .RequestGroup(key);
        }
        
        if(deassign == null)
        {
            deassign = key.Data.Get<UnitGroup>(GroupIds.First());
        }
        
        if(deassign != null) GroupIds.Remove(deassign.Id);
        return deassign;
    }

    public void SetTargets(LogicWriteKey key)
    {
        // var alliance = Regime.Entity(key.Data).GetAlliance(key.Data);
        // var wps = HeldCells(key.Data);
        // var targets = expand(wps);
        // // targets = expand(targets);
        // TargetAreaCellIds.Clear();
        // TargetAreaCellIds.AddRange(targets.Select(t => t.Id));
        // IEnumerable<Waypoint> expand(IEnumerable<Waypoint> source)
        // {
        //     return source
        //         .SelectMany(wp => wp.GetNeighbors(key.Data)
        //             .Where(n => n.IsHostile(alliance, key.Data)));
        // }
        // foreach (var seg in Assignments.OfType<FrontSegmentAssignment>())
        // {
        //     seg.SetAdvance(true, key);
        // }
    }

    public IEnumerable<PolyCell> GetCells(Data d)
    {
        return HeldCellIds.Select(id => PlanetDomainExt.GetPolyCell(id, d));
    }
}