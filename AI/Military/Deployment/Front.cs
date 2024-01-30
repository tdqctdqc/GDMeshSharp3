using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text.RegularExpressions;
using GDMeshSharp3.Exception;
using Godot;
using MathNet.Numerics.Statistics;
using Microsoft.FSharp.Data.UnitSystems.SI.UnitNames;

public class Front : CompoundDeploymentBranch
{
    
    public static float CoverOpposingWeight {get; private set;} = .5f;
    public static float CoverLengthWeight {get; private set;} = 1f;
    public static float DesiredOpposingPpRatio {get; private set;} = 2f;
    public static float PowerPointsPerCellFaceToCover {get; private set;} = 100f;
    public HashSet<int> HeldCellIds { get; private set; }
    public HashSet<int> TargetAreaCellIds { get; private set; }
    
    public Color Color { get; private set; }
    public Front(
        int id,
        ERef<Regime> regime, 
        HashSet<int> heldCellIds,
        HashSet<int> targetAreaCellIds,
        HashSet<DeploymentBranch> assignments,
        Color color) 
        : base(assignments, regime, id)
    {
        HeldCellIds = heldCellIds;
        TargetAreaCellIds = targetAreaCellIds;
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

    public void MakeSegments(LogicWriteKey key)
    {
        var d = key.Data;
        var lines = GetLines(d);
        if (lines.Count == 0)
        {
            Disband(key);
            return;
        }
        var newSegs = lines
            .Select(l => FrontSegmentAssignment
                .Construct(new ERef<Regime>(Regime.RefId), l, false, key)).ToList();
        foreach (var oldSeg in Assignments
                     .OfType<FrontSegmentAssignment>().ToArray())
        {
            oldSeg.DissolveInto(newSegs, key);
            oldSeg.Disband(key);
        }
        foreach (var fsa in newSegs)
        {
            fsa.SetParent(this, key);
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
    public override void DissolveInto(IEnumerable<DeploymentBranch> intos, LogicWriteKey key)
    {
        var d = key.Data;
        var fronts = intos.OfType<Front>();
        if (fronts.Count() == 0) throw new Exception();
        foreach (var assgn in Assignments.ToArray())
        {
            var wp = assgn.GetCharacteristicCell(d);
            var front = fronts.FirstOrDefault(t => t.HeldCellIds.Contains(wp.Id));
            if (front == null)
            {
                front = (Front)intos.MinBy(f => f.GetCharacteristicCell(d)
                    .GetCenter().GetOffsetTo(wp.GetCenter(), d).Length());
            }
            assgn.SetParent(front, key);
        }
    }
    public IEnumerable<PolyCell> GetCells(Data d)
    {
        return HeldCellIds.Select(id => PlanetDomainExt.GetPolyCell(id, d));
    }
}