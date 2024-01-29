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
    
    public static float CoverOpposingWeight {get; private set;} = .5f;
    public static float CoverLengthWeight {get; private set;} = 1f;
    public static float DesiredOpposingPpRatio {get; private set;} = 2f;
    public static float PowerPointsPerCellFaceToCover {get; private set;} = 100f;
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

        var oppNeed = opposing * DesiredOpposingPpRatio;
        var lengthNeed = length * PowerPointsPerCellFaceToCover;
        return Mathf.Max(oppNeed, lengthNeed);
    }
    public List<List<FrontFace<PolyCell>>> 
        GetLines(Data d)
    {
        var alliance = Regime.Entity(d).GetAlliance(d);
        var cells = GetCells(d);
        var lines = FrontFinder.FindPolyCellFront(
                cells, alliance, d);
        if (lines.Any(l => l.Count == 0))
        {
            throw new Exception();
        }

        return lines;
    }

    public override void ValidateGroups(LogicWriteKey key)
    {
        GroupIds.RemoveWhere(id => key.Data.EntitiesById.ContainsKey(id) == false);
        foreach (var fa in Assignments)
        {
            fa.ValidateGroups(key);
        }
    }
    public void Validate(LogicWriteKey key)
    {
        ValidateSegments(key);
        var segmentFaces = Assignments.OfType<FrontSegmentAssignment>()
            .SelectMany(s => s.Segment.Faces);
        if (segmentFaces.Count() != segmentFaces.Distinct().Count())
        {
            throw new Exception();
        }

        // Assignments.RemoveWhere(a => a.GroupIds.Count == 0);
        
        // MakeNewSegmentsForUncoveredFaces(key);
        TransferFacesBetweenSegments(key);
        //shift faces + groups between segments to make them good size
        //shift support groups + reserves
    }
    
    private void ValidateSegments(LogicWriteKey key)
    {
        var d = key.Data;
        var lines = GetLines(d);
        if (lines.Count == 0)
        {
            Assignments.Clear();
            GroupIds.Clear();
            return;
        }
        var newSegs = lines
            .Select(l => FrontSegmentAssignment
                .Construct(new EntityRef<Regime>(Regime.RefId), l, false, key)).ToList();
        foreach (var seg in Assignments
                     .OfType<FrontSegmentAssignment>().ToArray())
        {
            seg.PartitionAmong(newSegs, key);
        }
        Assignments.Clear();
        Assignments.AddRange(newSegs);
    }

    private void MakeNewSegmentsForUncoveredFaces(LogicWriteKey key)
    {
        var d = key.Data;
        var lines = GetLines(d);
        var facesHash = lines
            .SelectMany(l => l)
            .ToHashSet();
        var segments = Assignments
            .OfType<FrontSegmentAssignment>().ToList();
        var coveredFaces = 
            segments
            .SelectMany(s => s.Segment.Faces)
            .ToHashSet();
        var uncoveredFaces = facesHash.Except(coveredFaces).ToHashSet();
        
        foreach (var line in lines)
        {
            if (line.All(coveredFaces.Contains))
            {
                continue;
            }
            if (line.All(uncoveredFaces.Contains) == false)
            {
                line.DoForRuns(
                    v => uncoveredFaces.Contains(v),
                    l =>
                    {
                        var subLineSeg = FrontSegmentAssignment.Construct(
                            new EntityRef<Regime>(Regime.RefId),
                            l, false, key);
                        Assignments.Add(subLineSeg);
                    }
                );
                continue;
            }

            var newSegment = FrontSegmentAssignment.Construct(
                new EntityRef<Regime>(Regime.RefId),
                line, false, key);
            Assignments.Add(newSegment);
        }
    }

    private void TransferFacesBetweenSegments(LogicWriteKey key)
    {
        
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
        var bySatisfaction = Assignments
            .OrderBy(s => s.GetSatisfiedRatio(key.Data))
            .ToArray();
        for (var i = 0; i < bySatisfaction.Length; i++)
        {
            deassign = bySatisfaction[i].RequestGroup(key);
            if (deassign != null) break;
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
    public void DistributeInto(IEnumerable<FrontAssignment> intos, Data d)
    {
        foreach (var assgn in Assignments)
        {
            var wp = assgn.GetCharacteristicCell(d);
            var front = intos.FirstOrDefault(t => t.HeldCellIds.Contains(wp.Id));
            if (front == null)
            {
                front = intos.MinBy(f => f.GetCharacteristicCell(d)
                    .GetCenter().GetOffsetTo(wp.GetCenter(), d).Length());
            }
            front.Assignments.Add(assgn);
            front.GroupIds.AddRange(assgn.GroupIds);
            GroupIds.RemoveWhere(assgn.GroupIds.Contains);
        }

        foreach (var dissolveGroupId in GroupIds)
        {
            var group = d.Get<UnitGroup>(dissolveGroupId);
            var front = intos.FirstOrDefault(f => f.HeldCellIds.Contains(group.GetCell(d).Id));
            if (front is FrontAssignment == false)
            {
                front = intos
                    .MinBy(f =>
                        f.GetCharacteristicCell(d).GetCenter()
                            .GetOffsetTo(group.GetCell(d).GetCenter(), d));
            }

            front.GroupIds.Add(group.Id);
        }
    }
    public IEnumerable<PolyCell> GetCells(Data d)
    {
        return HeldCellIds.Select(id => PlanetDomainExt.GetPolyCell(id, d));
    }
}