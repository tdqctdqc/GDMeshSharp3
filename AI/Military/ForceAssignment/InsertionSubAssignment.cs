
using System;
using System.Collections.Generic;
using System.Linq;
using Godot;
using MessagePack;

public class InsertionSubAssignment
{
    public Dictionary<int, FrontFace<PolyCell>> Insertions { get; private set; }

    public static InsertionSubAssignment Construct()
    {
        return new InsertionSubAssignment(
            new Dictionary<int, FrontFace<PolyCell>>());
    }

    [SerializationConstructor] private InsertionSubAssignment(
        Dictionary<int, FrontFace<PolyCell>> insertions)
    {
        Insertions = insertions;
    }

    private Dictionary<Vector2I, FrontFace<PolyCell>> 
        GetInsertionCellsByGroupIds(FrontSegmentAssignment seg, 
            Data d)
    {
        var lineGroupIds = seg.HoldLine
            .GetGroupsInOrder(seg, d)
            .Select(g => g.Id)
            .ToList();
        if (lineGroupIds.Count == 0) return null;
        var insertionCellsByGroupIds = new Dictionary<Vector2I, FrontFace<PolyCell>>();
        insertionCellsByGroupIds.Add(new Vector2I(-1, 
                lineGroupIds.First()),
            seg.Segment.Faces.First());
        for (var i = 0; i < lineGroupIds.Count - 1; i++)
        {
            var prevGroupId = lineGroupIds[i];
            var nextGroupId = lineGroupIds[i + 1];
            var lastFaceOfPrev = seg.HoldLine.FacesByGroupId[prevGroupId].Last();
            insertionCellsByGroupIds.Add(new Vector2I(prevGroupId, nextGroupId),
                lastFaceOfPrev);
        }
        insertionCellsByGroupIds.Add(new Vector2I(lineGroupIds.Last(), -1),
            seg.Segment.Faces.Last());
        return insertionCellsByGroupIds;
    }
    public void Handle(FrontSegmentAssignment seg, LogicWriteKey key)
    {
        if (Insertions.Count == 0) return;
        var lineGroupIds = seg.HoldLine
            .GetGroupsInOrder(seg, key.Data).Select(g => g.Id).ToList();
        var insertionCellsByGroupIds = GetInsertionCellsByGroupIds(seg, key.Data);
        ValidateInsertionPoints(seg, key);
        GiveOrders(seg, key);
    }

    public void ValidateInsertionPoints(FrontSegmentAssignment seg,
        LogicWriteKey key)
    {
        var insertionCellsByGroupIds = GetInsertionCellsByGroupIds(seg, key.Data);
        foreach (var groupId in Insertions.Keys.ToList())
        {
            ValidateInsertionPoint(groupId, seg, insertionCellsByGroupIds, key.Data);
        }
    }
    private void ValidateInsertionPoint(
        int groupId,
        FrontSegmentAssignment seg, 
        Dictionary<Vector2I, FrontFace<PolyCell>> insertionCellsByGroupIds,
        Data d)
    {
        var group = d.Get<UnitGroup>(groupId);
        var old = Insertions[groupId];
        if (seg.Segment.Faces.Contains(old)) return;

        if (insertionCellsByGroupIds == null)
        {
            var close = insertionCellsByGroupIds
                .MinBy(kvp => kvp.Value.GetNative(d).GetCenter().GetOffsetTo(group.GetCell(d).GetCenter(), d).Length());
            Insertions[groupId] = close.Value;
        }
        else
        {
            var close = seg.Segment
                .Faces.MinBy(f => f.GetNative(d).GetCenter().GetOffsetTo(group.GetCell(d).GetCenter(), d).Length());
            Insertions[groupId] = close;
        }
    }
    private void GiveOrders(FrontSegmentAssignment seg, LogicWriteKey key)
    {
        foreach (var kvp in Insertions)
        {
            var group = key.Data.Get<UnitGroup>(kvp.Key);
            var destFace = kvp.Value;
            var destCell = destFace.GetNative(key.Data);
            if (group.GetCell(key.Data) == destCell)
            {
                seg.HoldLine.AddGroupToLine(seg, group, destFace);
                Insertions.Remove(group.Id);
            }
            else
            {
                var order = GoToCellGroupOrder.Construct(
                    destCell, seg.Regime.Entity(key.Data),
                    group, key.Data);
                key.SendMessage(new SetUnitOrderProcedure(group.MakeRef(), order));
            }
        }
    }
    public void InsertGroups(FrontSegmentAssignment seg,
        IEnumerable<UnitGroup> gs, 
        LogicWriteKey key)
    {
        var insertionCells
            = GetInsertionCellsByGroupIds(seg, key.Data);
        foreach (var group in gs)
        {
            FrontFace<PolyCell> close;
            if (insertionCells == null)
            {
                close = seg.Segment.Faces.MinBy(f => f.GetNative(key.Data).GetCenter()
                    .GetOffsetTo(group.GetCell(key.Data).GetCenter(), key.Data)
                    .Length());
            }
            else
            {
                close = insertionCells
                    .MinBy(kvp => kvp.Value.GetNative(key.Data).GetCenter().GetOffsetTo(group.GetCell(key.Data).GetCenter(), key.Data)
                        .Length()).Value;
            }
            Insertions[group.Id] = close;
        }
    }
    public void ValidateGroups(LogicWriteKey key)
    {
        var badIds = Insertions.Keys.Where(id => key.Data.EntitiesById.ContainsKey(id) == false).ToArray();
        foreach (var badId in badIds)
        {
            Insertions.Remove(badId);
        }
    }
    public void DistributeAmong(IEnumerable<FrontSegmentAssignment> segs, LogicWriteKey key)
    {
        foreach (var kvp in Insertions)
        {
            var groupId = kvp.Key;
            if (key.Data.EntitiesById.ContainsKey(groupId) == false)
            {
                continue;
            }
            var insertionFace = kvp.Value;
            var seg = segs
                .FirstOrDefault(s => s.Segment.Faces.Contains(insertionFace));
            if (seg == null)
            {
                var group = key.Data.Get<UnitGroup>(groupId);
                var groupCell = group.GetCell(key.Data);
                seg = segs.MinBy(s =>
                    s.GetCharacteristicCell(key.Data)
                        .GetCenter()
                        .GetOffsetTo(groupCell.GetCenter(), key.Data).Length());
                insertionFace = seg.Segment.Faces.MinBy(f => 
                    f.GetNative(key.Data).GetCenter()
                        .GetOffsetTo(groupCell.GetCenter(), key.Data).Length());
            }
            seg.GroupIds.Add(groupId);
            seg.Insert.Insertions.Add(groupId, insertionFace);
        }
        Insertions.Clear();
    }
}