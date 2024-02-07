using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text.RegularExpressions;
using GDMeshSharp3.Exception;
using Godot;
using MathNet.Numerics.Statistics;
using MessagePack;
using Microsoft.FSharp.Data.UnitSystems.SI.UnitNames;

public class Front : DeploymentTrunk
{
    
    public static float CoverOpposingWeight {get; private set;} = .5f;
    public static float CoverLengthWeight {get; private set;} = 1f;
    public static float DesiredOpposingPpRatio {get; private set;} = 2f;
    public static float PowerPointsPerCellFaceToCover {get; private set;} = 100f;
    public HashSet<int> HeldCellIds { get; private set; }
    public HashSet<int> TargetAreaCellIds { get; private set; }
    
    public Color Color { get; private set; }

    public static Front Construct(
        DeploymentAi ai,
        Regime r, 
        IEnumerable<PolyCell> cells,
        LogicWriteKey key)
    {
        var id = ai.DeploymentTreeIds.TakeId(key.Data);
        var reserve = ReserveAssignment.Construct(ai, id, r.MakeRef(), key.Data);
        var f = new Front(
            id,
            -1,
            r.MakeRef(),
            cells.Select(wp => wp.Id).ToHashSet(),
            new HashSet<int>(),
            new HashSet<DeploymentBranch>(),
            ColorsExt.GetRandomColor(), reserve
        );
        return f;
    }
    [SerializationConstructor] private Front(
        int id,
        int parentId,
        ERef<Regime> regime, 
        HashSet<int> heldCellIds,
        HashSet<int> targetAreaCellIds,
        HashSet<DeploymentBranch> branches,
        Color color, ReserveAssignment reserve) 
        : base(branches, regime, id, parentId, reserve)
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
        var length = Branches.OfType<FrontSegment>().Sum(s => s.GetLength(data));
        var oppNeed = opposing * DesiredOpposingPpRatio;
        var lengthNeed = length * PowerPointsPerCellFaceToCover;
        return Mathf.Max(oppNeed, lengthNeed);
    }
    public List<List<FrontFace>> 
        GetLines(Data d)
    {
        var alliance = Regime.Entity(d).GetAlliance(d);
        var cells = GetCells(d).ToHashSet();
        var lines = FrontFinder.FindFront(
                cells, d);
        if (lines.Any(l => l.Count == 0))
        {
            throw new Exception();
        }

        return lines;
    }

    public void MakeSegments(DeploymentAi ai, LogicWriteKey key)
    {
        var d = key.Data;
        var lines = GetLines(d);
        if (lines.Count == 0)
        {
            Disband(ai, key);
            return;
        }
        var oldSegs = Branches
            .OfType<FrontSegment>().ToArray();
        var newSegs = lines
            .Select(l => FrontSegment
                .Construct(ai, new ERef<Regime>(Regime.RefId), l, false, key)).ToList();
        foreach (var fsa in newSegs)
        {
            fsa.SetParent(ai, this, key);
            ai.AddNode(fsa);
        }
        foreach (var oldSeg in oldSegs)
        {
            oldSeg.DissolveInto(ai, this, newSegs, key);
            oldSeg.Disband(ai, key);
        }
    }


    private void TransferFacesBetweenSegments(LogicWriteKey key)
    {
        
    }

    public override PolyCell GetCharacteristicCell(Data d)
    {
        return HeldCellIds.Select(i => PlanetDomainExt.GetPolyCell(i, d))
            .First();
    }
    public override void DissolveInto(DeploymentAi ai, 
        DeploymentTrunk parent,
        IEnumerable<DeploymentBranch> intos, LogicWriteKey key)
    {
        var d = key.Data;
        var fronts = intos.OfType<Front>();
        if (fronts.Count() == 0)
        {
            throw new Exception();
        }
        foreach (var assgn in Branches.ToArray())
        {
            var wp = assgn.GetCharacteristicCell(d);
            var front = fronts.FirstOrDefault(t => t.HeldCellIds.Contains(wp.Id));
            if (front == null)
            {
                front = (Front)fronts.MinBy(f => f.GetCharacteristicCell(d)
                    .GetCenter().GetOffsetTo(wp.GetCenter(), d).Length());
            }
            assgn.SetParent(ai, front, key);
        }
    }

    public override Vector2 GetMapPosForDisplay(Data d)
    {
        var frontSegs = Branches.OfType<FrontSegment>();
        var axis = frontSegs
           .SelectMany(fs => fs.Frontline.Faces)
           .Select(f => f.GetAxis(d).Normalized())
           .Sum()
           .Normalized();
        var avgPos = d.Planet.GetAveragePosition(GetCells(d).Select(c => c.GetCenter()));
        return avgPos - axis * 100f;
    }

    public IEnumerable<PolyCell> GetCells(Data d)
    {
        return HeldCellIds.Select(id => PlanetDomainExt.GetPolyCell(id, d));
    }
}