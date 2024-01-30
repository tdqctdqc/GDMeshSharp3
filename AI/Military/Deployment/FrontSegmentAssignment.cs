using System;
using System.Collections.Generic;
using MathNet.Numerics;
using System.Linq;
using System.Reflection.Metadata;
using Godot;
using MessagePack;

public class FrontSegmentAssignment : DeploymentBranch
{
    public static int IdealSegmentLength = 5;
    public FrontSegment Segment { get; private set; }
    public HoldLineAssignment HoldLine { get; private set; }
    public ReserveAssignment Reserve { get; private set; }
    public InsertionAssignment Insert { get; private set; }
    public Color Color { get; private set; }
    public bool Attack { get; private set; }
    public static FrontSegmentAssignment Construct(
        ERef<Regime> r,
        List<FrontFace<PolyCell>> frontLine,
        bool attack,
        LogicWriteKey key)
    {
        var frontLineFaces = frontLine.ToList();
        var cells = frontLine.Select(face => face.GetNative(key.Data)).ToHashSet();
        var id = key.Data.HostLogicData.DeploymentTreeIds.TakeId(key.Data);
        
        var fsa = new FrontSegmentAssignment(
            id,
            new FrontSegment(frontLineFaces),
            r,
            attack,
            ColorsExt.GetRandomColor(),
            HoldLineAssignment.Construct(id, r, key),
            ReserveAssignment.Construct(id, r, key),
            InsertionAssignment.Construct(id, r, key)
        );
        return fsa;
    }
    [SerializationConstructor] private FrontSegmentAssignment(
        int id,
        FrontSegment segment,
        ERef<Regime> regime,
        bool attack,
        Color color,
        HoldLineAssignment holdLine,
        ReserveAssignment reserve,
        InsertionAssignment insert) 
            : base(regime, id)
    {
        Color = color;
        if (segment.Faces.Count == 0) throw new Exception();
        Attack = attack;
        Segment = segment;
        HoldLine = holdLine;
        Reserve = reserve;
        Insert = insert;
    }

    public override void DissolveInto(
        IEnumerable<DeploymentBranch> into,
        LogicWriteKey key)
    {
        var newSegs = into.OfType<FrontSegmentAssignment>();
        HoldLine.DistributeAmong(newSegs, key);
        Insert.DistributeAmong(newSegs, key);
        Reserve.DistributeAmong(newSegs, key);
    }

    public override void AdjustWithin(LogicWriteKey key)
    {
        Reserve.AdjustWithin(key);
        Insert.AdjustWithin(key);
        HoldLine.AdjustWithin(key);
    }

    public IEnumerable<PolyCell> GetCells(Data d)
    {
        return Segment.Faces
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

    
    public override UnitGroup GetPossibleTransferGroup(LogicWriteKey key)
    {
        if (Reserve.GetPossibleTransferGroup(key) is UnitGroup g1)
        {
            return g1;
        }
        if (Insert.GetPossibleTransferGroup(key) is UnitGroup g2)
        {
            return g2;
        }
        if (HoldLine.GetPossibleTransferGroup(key) is UnitGroup g3)
        {
            return g3;
        }

        return null;
    }

    public override IEnumerable<IDeploymentNode> Children()
    {
        return new IDeploymentNode[]{Reserve, HoldLine, Insert};
    }
    public int GetLength(Data d)
    {
        return Segment.Faces.Count;
    }
    public override PolyCell GetCharacteristicCell(Data d)
    {
        return GetCells(d).First();
    }
}