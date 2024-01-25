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
    public FrontSegment Segment { get; private set; }
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
            new FrontSegment(frontLineFaces),
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
        FrontSegment segment,
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
        if (segment.Faces.Count == 0) throw new Exception();
        Attack = attack;
        Segment = segment;
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
            .Except(HoldLine.FacesByGroupId.Keys)
            .Except(Insert.Insertions.Keys)
            .Except(Reserve.GroupIds)
            .Select(i => d.Get<UnitGroup>(i));
    }
    

    private void MoveDistantGroupsToFront(LogicWriteKey key)
    {
        var freeGroups = GetFreeGroups(key.Data);
        Insert.InsertGroups(this, freeGroups, key);
    }
    public IEnumerable<FrontSegmentAssignment> ValidateFaces
        (   List<List<FrontFace<PolyCell>>> frontLines,
            HashSet<FrontFace<PolyCell>> frontFaces,
            HashSet<FrontSegmentAssignment> allSegs,
            LogicWriteKey key)
    {
        if (Segment.Faces.All(frontFaces.Contains)) return this.Yield();
        
        
        //first try to reunite front
            //if so then validate subassgns
        var otherSegFaces = allSegs.Except(this.Yield()).SelectMany(s => s.Segment.Faces).ToHashSet();
        var reunited = Segment.CheckReunite(frontLines, 
            frontFaces, 
            otherSegFaces,
            key, out var res);
        if (reunited)
        {
            HoldLine.ValidateGroupFaces(this, key);
            Insert.ValidateInsertionPoints(this, key);
            Reserve.Validate(this, key);
            return this.Yield();
        }

        var newSegs = res.Select(
            r => FrontSegmentAssignment.Construct(new EntityRef<Regime>(Regime.RefId),
                r, false, key)).ToList();
        var freeGroups = GetFreeGroups(key.Data);
        HoldLine.DistributeAmong(newSegs, key);
        Insert.DistributeAmong(newSegs, key);
        Reserve.DistributeAmong(newSegs, key);
        foreach (var freeGroup in freeGroups)
        {
            var groupCell = freeGroup.GetCell(key.Data);
            var close = newSegs
                .MinBy(s =>
                    s.GetCharacteristicCell(key.Data)
                        .GetCenter()
                        .GetOffsetTo(groupCell.GetCenter(), key.Data));
            close.GroupIds.Add(freeGroup.Id);
        }
        
        
        
        return newSegs;
    }

    private void PartitionAmong(IEnumerable<FrontSegmentAssignment> newSegs,
        LogicWriteKey key)
    {
        foreach (var groupId in GroupIds)
        {
            if (HoldLine.FacesByGroupId.ContainsKey(groupId))
            {
                
            }
            else if (Reserve.GroupIds.Contains(groupId))
            {
                
            }
            else if (Insert.Insertions.ContainsKey(groupId))
            {
                
            }
        }
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

        var oppNeed = opposing * FrontAssignment.DesiredOpposingPpRatio;
        var lengthNeed = length * FrontAssignment.PowerPointsPerCellFaceToCover;

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
    
    public void MergeInto(FrontSegmentAssignment merging, LogicWriteKey key)
    {
        GroupIds.AddRange(merging.GroupIds);
    }

    
    public override UnitGroup RequestGroup(LogicWriteKey key)
    {
        if (GroupIds.Count < 2) return null;
        
        var freeGroups = GetFreeGroups(key.Data);
        int de = -1;
        if (freeGroups.Count() > 0)
        {
            de = freeGroups.First().Id;
        }
        else if(Reserve.GroupIds.Count() > 0)
        {
            de = Reserve.GroupIds.First();
            Reserve.GroupIds.Remove(de);
        }
        else if(Insert.Insertions.Count() > 0)
        {
            de = Insert.Insertions.First().Key;
            Insert.Insertions.Remove(de);
        }
        else if (HoldLine.FacesByGroupId.Count > 1)
        {
            var lineGroups = HoldLine.FacesByGroupId
                .Select(kvp => key.Data.Get<UnitGroup>(kvp.Key));
            var numUnits = lineGroups.Sum(g => g.Units.Count());
            var smallest = lineGroups.MinBy(g => g.Units.Count());
            if (numUnits - smallest.Units.Count() >= Segment.Faces.Count)
            {
                HoldLine.FacesByGroupId.Remove(smallest.Id);
                de = smallest.Id;
            }
        }

        if (de != -1)
        {
            var group = key.Data.Get<UnitGroup>(de);
            GroupIds.Remove(de);
            return group;
        }
        return null;
    }

    public override void TakeAwayGroup(UnitGroup g, LogicWriteKey key)
    {
        Insert.Insertions.Remove(g.Id);
        HoldLine.FacesByGroupId.Remove(g.Id);
        Reserve.GroupIds.Remove(g.Id);
        GroupIds.Remove(g.Id);
    }

    public override void AssignGroups(LogicWriteKey key)
    {
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