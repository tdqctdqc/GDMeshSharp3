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
    public IEnumerable<FrontSegmentAssignment> ValidateFaces
        (   List<List<FrontFace<PolyCell>>> frontLines,
            HashSet<FrontFace<PolyCell>> frontFaces,
            LogicWriteKey key)
    {
        if (FrontLineFaces.All(frontFaces.Contains)) return this.Yield();
        return this.Yield();
        //
        // var d = key.Data;
        // var regime = Regime.Entity(d);
        // var alliance = regime.GetAlliance(d);
        // var newFronts = new List<List<FrontFace<PolyCell>>>();
        //
        // var validFacesHash = FrontLineFaces.Where()
        //

    }

    private void PartitionAmong(IEnumerable<FrontSegmentAssignment> newSegs,
        LogicWriteKey key)
    {
        foreach (var groupId in GroupIds)
        {
            if (HoldLine.BoundsByGroupId.ContainsKey(groupId))
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
        return FrontLineFaces
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
        else if (HoldLine.BoundsByGroupId.Count > 1)
        {
            var lineGroups = HoldLine.BoundsByGroupId
                .Select(kvp => key.Data.Get<UnitGroup>(kvp.Key));
            var numUnits = lineGroups.Sum(g => g.Units.Count());
            var smallest = lineGroups.MinBy(g => g.Units.Count());
            if (numUnits - smallest.Units.Count() >= FrontLineFaces.Count)
            {
                HoldLine.BoundsByGroupId.Remove(smallest.Id);
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