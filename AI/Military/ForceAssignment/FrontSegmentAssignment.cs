using System;
using System.Collections.Generic;
using MathNet.Numerics;
using System.Linq;
using System.Reflection.Metadata;
using Godot;
using MessagePack;

public class FrontSegmentAssignment : ForceAssignment
{
    public static int IdealSegmentLength = 5;
    public List<FrontFace<PolyCell>> FrontLineFaces { get; private set; }
    public HoldLineSubAssignment HoldLine { get; private set; }
    public ReserveSubAssignment Reserve { get; private set; }
    public InsertionSubAssignment Insert { get; private set; }
    public Color Color { get; private set; }
    public bool Attack { get; private set; }
    public static FrontSegmentAssignment Construct(
        EntityRef<Regime> r,
        List<FrontFace<PolyCell>> frontLine,
        bool attack,
        LogicWriteKey key)
    {
        var frontLineFaces = frontLine.ToList();
        var cells = frontLine.Select(face => face.GetNative(key.Data)).ToHashSet();
        var fsa = new FrontSegmentAssignment(
            key.Data.IdDispenser.TakeId(),
            frontLineFaces,
            new HashSet<int>(),
            r,
            attack,
            ColorsExt.GetRandomColor(),
            HoldLineSubAssignment.Construct(),
            ReserveSubAssignment.Construct(),
            InsertionSubAssignment.Construct()
        );
        return fsa;
    }
    [SerializationConstructor] private FrontSegmentAssignment(
        int id,
        List<FrontFace<PolyCell>> frontLineFaces,
        HashSet<int> groupIds, 
        EntityRef<Regime> regime,
        bool attack,
        Color color,
        HoldLineSubAssignment holdLine,
        ReserveSubAssignment reserve,
        InsertionSubAssignment insert
        ) 
        : base(groupIds, regime, id)
    {
        Color = color;
        if (frontLineFaces.Count == 0) throw new Exception();
        Attack = attack;
        FrontLineFaces = frontLineFaces;
        HoldLine = holdLine;
        Reserve = reserve;
        Insert = insert;
    }
    
    public override void CalculateOrders(MinorTurnOrders orders,
        LogicWriteKey key)
    {
        if (GroupIds.Count() == 0) return;
        MoveDistantGroupsToFront(key);
        Insert.Handle(this, key);
        HoldLine.Handle(this, key);
    }

    private IEnumerable<UnitGroup> GetFreeGroups(Data d)
    {
        return GroupIds
            .Except(HoldLine.BoundsByGroupId.Keys)
            .Except(Insert.Insertions.Keys)
            .Except(Reserve.GroupIds)
            .Select(i => d.Get<UnitGroup>(i));
    }
    

    private void MoveDistantGroupsToFront(LogicWriteKey key)
    {
        var freeGroups = GetFreeGroups(key.Data);
        Insert.InsertGroups(this, freeGroups, key);
    }
    public IEnumerable<FrontSegmentAssignment> Correct(Data d)
    {
        return this.Yield();
    }
    
    public IEnumerable<PolyCell> GetCells(Data d)
    {
        return FrontLineFaces
            .Select(face => PlanetDomainExt.GetPolyCell(face.Native, d)).Distinct();
    }
    public override float GetPowerPointNeed(Data data)
    {
        var opposing = GetOpposingPowerPoints(data);
        var length = GetLength(data);

        return opposing * FrontAssignment.CoverOpposingWeight * FrontAssignment.DesiredOpposingPpRatio
               + length * FrontAssignment.CoverLengthWeight * FrontAssignment.PowerPointsPerCellFaceToCover;
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
    
    public void MergeInto(FrontSegmentAssignment merging, LogicWriteKey key)
    {
        GroupIds.AddRange(merging.GroupIds);
    }

    
    public override UnitGroup RequestGroup(LogicWriteKey key)
    {
        return null;
        if (GroupIds.Count < 2) return null;
        var freeGroups = GetFreeGroups(key.Data);
        if (freeGroups.Count() > 0)
        {
            var de = freeGroups.First();
            GroupIds.Remove(de.Id);
            return de;
        }
        else
        {
            return null;
        }
    }

    public override void TakeAwayGroup(UnitGroup g, LogicWriteKey key)
    {
        Insert.Insertions.Remove(g.Id);
        HoldLine.BoundsByGroupId.Remove(g.Id);
        Reserve.GroupIds.Remove(g.Id);
        GroupIds.Remove(g.Id);
    }

    public override void AssignGroups(LogicWriteKey key)
    {
    }

    public int GetLength(Data d)
    {
        return FrontLineFaces.Count;
    }

    
    public override PolyCell GetCharacteristicCell(Data d)
    {
        return GetCells(d).First();
    }
}