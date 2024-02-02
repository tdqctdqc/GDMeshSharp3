using System;
using System.Collections.Generic;
using MathNet.Numerics;
using System.Linq;
using System.Reflection.Metadata;
using Godot;
using MessagePack;

public class FrontSegment : DeploymentBranch
{
    public static int IdealSegmentLength = 5;
    public Frontline Frontline { get; private set; }
    public HoldLineAssignment HoldLine { get; private set; }
    public InsertionAssignment Insert { get; private set; }
    public Color Color { get; private set; }
    public bool Attack { get; private set; }
    public static FrontSegment Construct(
        DeploymentAi ai,
        ERef<Regime> r,
        List<FrontFace<PolyCell>> frontLine,
        bool attack,
        LogicWriteKey key)
    {
        var frontLineFaces = frontLine.ToList();
        var cells = frontLine.Select(face => face.GetNative(key.Data)).ToHashSet();
        var id = ai.DeploymentTreeIds.TakeId(key.Data);
        
        var fsa = new FrontSegment(
            id,
            -1,
            new Frontline(frontLineFaces),
            r,
            attack,
            ColorsExt.GetRandomColor(),
            HoldLineAssignment.Construct(ai, id, r, key),
            ReserveAssignment.Construct(ai, id, r, key.Data),
            InsertionAssignment.Construct(ai, id, r, key)
        );
        return fsa;
    }
    [SerializationConstructor] private FrontSegment(
        int id,
        int parentId,
        Frontline frontline,
        ERef<Regime> regime,
        bool attack,
        Color color,
        HoldLineAssignment holdLine,
        ReserveAssignment reserve,
        InsertionAssignment insert) 
            : base(regime, id, parentId, reserve)
    {
        Color = color;
        if (frontline.Faces.Count == 0) throw new Exception();
        Attack = attack;
        Frontline = frontline;
        HoldLine = holdLine;
        Insert = insert;
    }

    public override void DissolveInto(DeploymentAi ai,
        IEnumerable<DeploymentBranch> into,
        LogicWriteKey key)
    {
        var newSegs = into.OfType<FrontSegment>();
        HoldLine.DissolveInto(ai, newSegs, key);
        Insert.DissolveInto(ai, newSegs, key);
        Reserve.DissolveInto(ai, newSegs, key);
    }

    public override Vector2 GetMapPosForDisplay(Data d)
    {
        return d.Planet.GetAveragePosition(Frontline.Faces.Select(f => f.GetNative(d).GetCenter()));
    }
    public IEnumerable<PolyCell> GetCells(Data d)
    {
        return Frontline.Faces
            .Select(face => PlanetDomainExt.GetPolyCell(face.Native, d)).Distinct();
    }
    public override float GetPowerPointNeed(Data data)
    {
        var opposing = GetOpposingPowerPoints(data);
        var length = GetLength(data);

        var oppNeed = opposing * Front.DesiredOpposingPpRatio;
        var lengthNeed = length * Front.PowerPointsPerCellFaceToCover;

        return Mathf.Max(oppNeed, lengthNeed);
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
                if (us == null) return 0f;
                return us.Sum(u => u.GetPowerPoints(data));
            });
    }

    
    public override bool PullGroup(DeploymentAi ai, GroupAssignment transferTo,
        LogicWriteKey key)
    {
        if (Reserve.PullGroup(ai, transferTo, key))
        {
            return true;
        }
        if (Insert.PullGroup(ai, transferTo, key))
        {
            return true;
        }
        if (HoldLine.PullGroup(ai, transferTo, key))
        {
            return true;
        }

        return false;
    }

    public override bool PushGroup(DeploymentAi ai, GroupAssignment transferFrom, LogicWriteKey key)
    {
        return transferFrom.PullGroup(ai, Reserve, key);
    }

    public override IEnumerable<IDeploymentNode> Children()
    {
        return new IDeploymentNode[]{Reserve, HoldLine, Insert};
    }
    public int GetLength(Data d)
    {
        return Frontline.Faces.Count;
    }
    public override PolyCell GetCharacteristicCell(Data d)
    {
        return GetCells(d).First();
    }
}