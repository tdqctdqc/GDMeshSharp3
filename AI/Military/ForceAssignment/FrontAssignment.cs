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
    
    public static float CoverOpposingWeight = 0f;//2f;
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
        // this.ShiftGroups(key);
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

    
    public void MergeInto(FrontAssignment dissolve, LogicWriteKey key)
    {
        GroupIds.AddRange(dissolve.GroupIds);
        Assignments.AddRange(dissolve.Assignments);
    }


    public void CheckSegments(LogicWriteKey key)
    {
        // ExpandExistingSegmentsOverGaps(key);
        MakeNewSegmentsForUncoveredFaces(key);
        TransferFacesBetweenSegments(key);
        //shift faces + groups between segments to make them good size
        //shift support groups + reserves
    }
    
    private void ExpandExistingSegmentsOverGaps(LogicWriteKey key)
    {
        var d = key.Data;
        var faces = GetLines(d)
            .SelectMany(l => l)
            .ToHashSet();
        var segments = Assignments
            .OfType<FrontSegmentAssignment>().ToList();
        foreach (var seg in segments)
        {
            Assignments.Remove(seg);
            var newSegs = seg.Correct(d);
            Assignments.AddRange(newSegs);
        }
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
            .SelectMany(s => s.FrontLineFaces)
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
                var sublineStart = -1;
                for (var i = 0; i < line.Count; i++)
                {
                    var face = line[i];
                    if (uncoveredFaces.Contains(face) == false)
                    {
                        addSegment(sublineStart, i - 1);
                        sublineStart = -1;
                    }
                    else if (sublineStart == -1)
                    {
                        sublineStart = i;
                    }

                    if (i == line.Count() - 1)
                    {
                        addSegment(sublineStart, i);
                    }
                }

                void addSegment(int start, int end)
                {
                    if (start == -1) return;
                    var subLine = line.GetRange(start, end - start + 1);
                    var subLineSeg = FrontSegmentAssignment.Construct(
                        new EntityRef<Regime>(Regime.RefId),
                        subLine, false, key);
                    Assignments.Add(subLineSeg);
                }
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
        return null;
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