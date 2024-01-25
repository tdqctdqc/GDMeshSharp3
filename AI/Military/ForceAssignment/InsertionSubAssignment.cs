
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
            .GetGroupsInOrder(d)
            .Select(g => g.Id)
            .ToList();
        if (lineGroupIds.Count == 0) return null;
        var insertionCellsByGroupIds = new Dictionary<Vector2I, FrontFace<PolyCell>>();
        insertionCellsByGroupIds.Add(new Vector2I(-1, 
                lineGroupIds.First()),
            seg.FrontLineFaces.First());
        for (var i = 0; i < lineGroupIds.Count - 1; i++)
        {
            var prevGroupId = lineGroupIds[i];
            var nextGroupId = lineGroupIds[i + 1];
            var lastIndexOfPrev = seg.HoldLine.BoundsByGroupId[prevGroupId].Y;
            var lastOfPrev = seg.FrontLineFaces[lastIndexOfPrev];
            insertionCellsByGroupIds.Add(new Vector2I(prevGroupId, nextGroupId),
                lastOfPrev);
        }
        insertionCellsByGroupIds.Add(new Vector2I(lineGroupIds.Last(), -1),
            seg.FrontLineFaces.Last());
        return insertionCellsByGroupIds;
    }
    public void Handle(FrontSegmentAssignment seg, LogicWriteKey key)
    {
        if (Insertions.Count == 0) return;
        var lineGroupIds = seg.HoldLine
            .GetGroupsInOrder(key.Data).Select(g => g.Id).ToList();
        var insertionCellsByGroupIds = GetInsertionCellsByGroupIds(seg, key.Data);
        
        foreach (var kvp in 
                 Insertions.ToList())
        {
            ValidateInsertionPoint(kvp.Key, seg, 
                insertionCellsByGroupIds, lineGroupIds, key.Data);
        }
        GiveOrders(seg, key);
    }

    private void ValidateInsertionPoint(
        int groupId,
        FrontSegmentAssignment seg, 
        Dictionary<Vector2I, FrontFace<PolyCell>> insertionCellsByGroupIds,
        List<int> lineGroupIds,
        Data d)
    {
        var group = d.Get<UnitGroup>(groupId);
        var old = Insertions[groupId];
        if (seg.FrontLineFaces.Contains(old)) return;

        if (insertionCellsByGroupIds == null)
        {
            var close = insertionCellsByGroupIds
                .MinBy(kvp => kvp.Value.GetNative(d).GetCenter().GetOffsetTo(group.GetCell(d).GetCenter(), d).Length());
            Insertions[groupId] = close.Value;
        }
        else
        {
            var close = seg.FrontLineFaces
                .MinBy(f => f.GetNative(d).GetCenter().GetOffsetTo(group.GetCell(d).GetCenter(), d).Length());
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
                close = seg.FrontLineFaces.MinBy(f => f.GetNative(key.Data).GetCenter()
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
}