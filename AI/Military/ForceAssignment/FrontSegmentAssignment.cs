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
    public List<int> FrontFaceGroupIds { get; private set; }
    public Dictionary<int, FrontFace<PolyCell>?> InsertingGroups { get; private set; }
    public HashSet<(int withdrawing, int replacing)> WithdrawingGroups { get; private set; }
    public HashSet<int> Reserve { get; private set; }
    public Color Color { get; private set; }
    public bool Attack { get; private set; }
    public static FrontSegmentAssignment Construct(
        EntityRef<Regime> r,
        List<FrontFace<PolyCell>> frontLine,
        bool attack,
        LogicWriteKey key)
    {
        var frontLineFaceIds = frontLine.ToList();
        var cells = frontLine.Select(face => face.GetNative(key.Data)).ToHashSet();
        var fsa = new FrontSegmentAssignment(
            key.Data.IdDispenser.TakeId(),
            frontLineFaceIds,
            Enumerable.Range(0, frontLineFaceIds.Count)
                .Select(i => -1).ToList(),
            new HashSet<int>(),
            r,
            attack,
            ColorsExt.GetRandomColor(),
            new Dictionary<int, FrontFace<PolyCell>?>(),
            new HashSet<(int withdrawing, int replacing)>(),
            new HashSet<int>());
        return fsa;
    }
    [SerializationConstructor] private FrontSegmentAssignment(
        int id,
        List<FrontFace<PolyCell>> frontLineFaces,
        List<int> frontFaceGroupIds,
        HashSet<int> groupIds, 
        EntityRef<Regime> regime,
        bool attack,
        Color color,
        Dictionary<int, FrontFace<PolyCell>?> insertingGroups,
        HashSet<(int withdrawing, int replacing)> withdrawingGroups,
        HashSet<int> reserve) 
        : base(groupIds, regime, id)
    {
        Color = color;
        if (frontLineFaces.Count == 0) throw new Exception();
        FrontFaceGroupIds = frontFaceGroupIds;
        Attack = attack;
        FrontLineFaces = frontLineFaces;
        InsertingGroups = insertingGroups;
        WithdrawingGroups = withdrawingGroups;
        Reserve = reserve;
    }
    
    public override void CalculateOrders(MinorTurnOrders orders,
        LogicWriteKey key)
    {
        if (GroupIds.Count() == 0) return;
        MoveDistantGroupsToFront(key);
        HandleInsertions(key);
        HandleCyclingOut(key);
        // if (FrontFaceGroupIds.All(id => id == -1))
        // {
        //     DoNewGroupToFaceAssignment(key.Data);
        // }
        // else
        // {
        //      AdjustFaceGroups(key.Data);
        // }
        
        AdjustFaceGroups(key.Data);
        GiveLineOrders(key);
    }

    private IEnumerable<UnitGroup> GetFreeGroups(Data d)
    {
        return GroupIds
            .Except(FrontFaceGroupIds)
            .Except(InsertingGroups.Select(v => v.Key))
            .Except(WithdrawingGroups.Select(w => w.withdrawing))
            .Except(WithdrawingGroups.Select(w => w.replacing))
            .Select(id => d.Get<UnitGroup>(id));
    }
    private void HandleInsertions(LogicWriteKey key)
    {
        //HANDLE GROUPS READY TO INSERT
        var insertableFaces = getInsertableFaces();
        foreach (var (id, oldFace) in InsertingGroups.ToList())
        {
            var group = key.Data.Get<UnitGroup>(id);
            var groupCell = group.GetCell(key.Data);
            if (insertableFaces.Any(f => f.Native == groupCell.Id))
            {
                var face = insertableFaces.First(f => f.Native == groupCell.Id);
                InsertingGroups.Remove(id);
                var index = FrontLineFaces.IndexOf(face);
                FrontFaceGroupIds[index] = group.Id;
            }
        }
        
        
        //ORDER FREE GROUPS TO INSERT
        foreach (var freeGroup in GetFreeGroups(key.Data))
        {
            InsertingGroups.Add(freeGroup.Id, null);
        }
        if (InsertingGroups.Count() == 0) return;
        var segCell = GetCharacteristicCell(key.Data);
        var byDist = InsertingGroups
            .Select(v => key.Data.Get<UnitGroup>(v.Key))
            .OrderBy(g => g.GetCell(key.Data).GetCenter()
                .GetOffsetTo(segCell.GetCenter(), key.Data).Length())
            .ToList();

        var insertionFaces = getInsertableFaces();
        foreach (var g in byDist)
        {
            if (insertionFaces.Count() == 0)
            {
                sendOrder(segCell, g);
                InsertingGroups.Remove(g.Id);
                continue;
            }
            var cell = g.GetCell(key.Data);
            var closest = insertionFaces
                .MinBy(f => cell.GetCenter().GetOffsetTo(f.GetNative(key.Data).GetCenter(), key.Data).Length());
            sendOrder(closest.GetNative(key.Data), g);
            InsertingGroups[g.Id] = closest;
            insertionFaces.Remove(closest);
        }

        void sendOrder(PolyCell cell, UnitGroup group)
        {
            var order = GoToCellGroupOrder.Construct(
                cell, Regime.Entity(key.Data),
                group, key.Data);
            key.SendMessage(new SetUnitOrderProcedure(group.MakeRef(), order));
        }

        HashSet<FrontFace<PolyCell>> getInsertableFaces()
        {
            var insertableFaces = new HashSet<FrontFace<PolyCell>>();
            for (var i = 0; i < FrontLineFaces.Count; i++)
            {
                var face = FrontLineFaces[i];
                var groupId = FrontFaceGroupIds[i];
                var insert = (groupId == -1)
                             || (getPrevGroupId(i) != groupId
                                 && getNextGroupId(i) == groupId)
                             || (getPrevGroupId(i) == groupId
                                 && getNextGroupId(i) != groupId);
                if (insert)
                {
                    insertableFaces.Add(face);
                }
                int getPrevGroupId(int index) => index > 0 ? FrontFaceGroupIds[index - 1] : -1;
                int getNextGroupId(int index) => index < FrontLineFaces.Count - 1 
                    ? FrontFaceGroupIds[index + 1] : -1;
            }

            return insertableFaces;
        }
    }
    private void HandleCyclingOut(LogicWriteKey key)
    {
        
    }
    // private void DoNewGroupToFaceAssignment(Data d)
    // {
    //     var nativeCells = FrontLineFaces
    //         .Select(f => PlanetDomainExt.GetPolyCell(f.Native, d))
    //         .ToHashSet();
    //     var readyGroups = GroupIds
    //         .Select(id => d.Get<UnitGroup>(id))
    //         .Where(g => nativeCells.Intersect(g.Units.Items(d).Select(u => u.Position.GetCell(d))).Count() > 0);
    //     if (readyGroups.Count() == 0) return;
    //     var alliance = Regime.Entity(d).GetAlliance(d);
    //     
    //     var assgns =
    //         Assigner.PickBestAndAssignAlongFaces<UnitGroup, FrontFace<PolyCell>>(
    //             FrontLineFaces,
    //             readyGroups.ToList(),
    //             g => g.GetPowerPoints(d),
    //             (g, f) => 
    //                 g.GetCell(d).GetCenter().GetOffsetTo(
    //                     PlanetDomainExt.GetPolyCell(f.Native, d).GetCenter(), d).Length(),
    //             readyGroups.Sum(g => g.GetPowerPoints(d)),
    //             f =>
    //             {
    //                 var foreignCell = PlanetDomainExt.GetPolyCell(f.Foreign, d);
    //                 if (foreignCell.Controller.RefId == -1) return 0f;
    //                 var foreignRegime = foreignCell.Controller.Entity(d);
    //                 var foreignAlliance = foreignRegime.GetAlliance(d);
    //
    //                 var units = foreignCell.GetUnits(d);
    //                 if (units == null || units.Count == 0) return FrontAssignment.PowerPointsPerCellFaceToCover;
    //
    //                 if (alliance.Rivals.Contains(foreignAlliance) == false) return 0f;
    //                 float mult = 1f;
    //                 if (alliance.AtWar.Contains(foreignAlliance)) mult = 2f;
    //                 return units.Sum(u => u.GetPowerPoints(d)) * mult;
    //             }
    //         );
    //     foreach (var (unitGroup, faces) in assgns)
    //     {
    //         var first = FrontLineFaces.IndexOf(faces.First());
    //         var last = FrontLineFaces.IndexOf(faces.Last());
    //         if (first > last) throw new Exception();
    //         for (var i = first; i <= last; i++)
    //         {
    //             FrontFaceGroupIds[i] = unitGroup.Id;
    //         }
    //     }
    // }
    private List<UnitGroup> GetLineGroups(Data d)
    {
        var readyGroups = new List<UnitGroup>();
        for (var i = 0; i < FrontFaceGroupIds.Count; i++)
        {
            var groupId = FrontFaceGroupIds[i];
            if (groupId == -1) continue;
            if (i > 0 && FrontFaceGroupIds[i - 1] == groupId)
            {
                continue;
            }
            readyGroups.Add(d.Get<UnitGroup>(groupId));
        }
        if (readyGroups.Distinct().Count() != readyGroups.Count())
        {
            throw new Exception();
        }
        return readyGroups;
    }
    private void AdjustFaceGroups(Data d)
    {
        var lineGroups = GetLineGroups(d);
        if (lineGroups.Count() == 0) return;
        
        var alliance = Regime.Entity(d).GetAlliance(d);
        var assgns =
            Assigner.PickInOrderAndAssignAlongFaces<UnitGroup, FrontFace<PolyCell>>(
                FrontLineFaces,
                lineGroups.ToList(),
                g => g.GetPowerPoints(d),
                lineGroups.Sum(g => g.GetPowerPoints(d)),
                f =>
                {
                    var foreignCell = PlanetDomainExt.GetPolyCell(f.Foreign, d);
                    if (foreignCell.Controller.RefId == -1) return 0f;
                    var foreignRegime = foreignCell.Controller.Entity(d);
                    var foreignAlliance = foreignRegime.GetAlliance(d);

                    var units = foreignCell.GetUnits(d);
                    if (units == null || units.Count == 0) return FrontAssignment.PowerPointsPerCellFaceToCover;

                    if (alliance.Rivals.Contains(foreignAlliance) == false) return 0f;
                    float mult = 1f;
                    if (alliance.AtWar.Contains(foreignAlliance)) mult = 2f;
                    return units.Sum(u => u.GetPowerPoints(d)) * mult;
                }
            );

        var newLineGroups = GetLineGroups(d);
        if (newLineGroups.Count != lineGroups.Count)
        {
            throw new Exception();
        }
        for (var i = 0; i < newLineGroups.Count; i++)
        {
            if (newLineGroups[i] != lineGroups[i])
            {
                throw new Exception();
            }
        }
        
        foreach (var (unitGroup, faces) in assgns)
        {
            var first = FrontLineFaces.IndexOf(faces.First());
            var last = FrontLineFaces.IndexOf(faces.Last());
            if (first > last) throw new Exception();
            for (var i = first; i <= last; i++)
            {
                FrontFaceGroupIds[i] = unitGroup.Id;
            }
        }
    }
    
    private void GiveLineOrders(LogicWriteKey key)
    {
        var group = -1;
        var from = -1;
        for (var i = 0; i < FrontFaceGroupIds.Count; i++)
        {
            var currGroup = FrontFaceGroupIds[i];
            if (currGroup != group)
            {
                giveLineOrder(group, from, i - 1);
                group = currGroup;
                from = i;
            }
            if (i == FrontFaceGroupIds.Count - 1)
            {
                giveLineOrder(group, from, i);
            }
        }

        void giveLineOrder(int groupId, int fromIndex, int toIndex)
        {
            if (groupId == -1) return;
            var line = FrontLineFaces.GetRange(fromIndex, toIndex - fromIndex + 1);
            var order = new DeployOnLineGroupOrder(line, false);
            var proc = new SetUnitOrderProcedure(new EntityRef<UnitGroup>(groupId),
                order);
            key.SendMessage(proc);
        }
    }

    private void MoveDistantGroupsToFront(LogicWriteKey key)
    {
        var alliance = Regime.Entity(key.Data).GetAlliance(key.Data);
        var lineGroups = FrontFaceGroupIds
            .ToHashSet();
        var unoccupiedGroups = GroupIds
            .Except(lineGroups)
            .Select(id => key.Data.Get<UnitGroup>(id));
        
        var frontlineCells = GetCells(key.Data).ToHashSet();
        
        foreach (var unreadyGroup in unoccupiedGroups)
        {
            var moveType = unreadyGroup.MoveType(key.Data);
            var dest = PathFinder<PolyCell>
                .FindClosest(
                    unreadyGroup.GetCell(key.Data),
                    frontlineCells.Contains,
                    c => c.GetNeighbors(key.Data).Where(n => moveType.Passable(n, alliance, key.Data)),
                    (p, q) => unreadyGroup.MoveType(key.Data).EdgeCost(p, q, key.Data));
            var order = GoToCellGroupOrder.Construct(
                dest, 
                unreadyGroup.Regime.Entity(key.Data),
                unreadyGroup, key.Data);
            var proc = new SetUnitOrderProcedure(unreadyGroup.MakeRef(), order);
            key.SendMessage(proc);
        }
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
        var notInLine = GroupIds
            .Except(FrontFaceGroupIds);
        if (notInLine.Count() > 0)
        {
            var de = notInLine.First();
            GroupIds.Remove(de);
            return key.Data.Get<UnitGroup>(de);
        }
        else
        {
            return null;
        }
    }

    public override void TakeAwayGroup(UnitGroup g, LogicWriteKey key)
    {
        for (var i = 0; i < FrontFaceGroupIds.Count; i++)
        {
            if (FrontFaceGroupIds[i] == g.Id)
            {
                FrontFaceGroupIds[i] = -1;
            }
        }
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