
using System;
using System.Collections.Generic;
using System.Linq;
using Godot;
using MessagePack;

public class InsertionAssignment : DeploymentLeaf
{

    public Dictionary<ERef<UnitGroup>, FrontFace<PolyCell>> Insertions { get; private set; }

    public static InsertionAssignment Construct(int parentId,
        ERef<Regime> regime, LogicWriteKey key)
    {
        return new InsertionAssignment(
            key.Data.HostLogicData.DeploymentTreeIds.TakeId(key.Data),
            parentId, regime, new UnitGroupManager(),
            new Dictionary<ERef<UnitGroup>, FrontFace<PolyCell>>());
    }

    [SerializationConstructor] private InsertionAssignment(
        int id, int parentId, ERef<Regime> regime, UnitGroupManager groups,
        Dictionary<ERef<UnitGroup>, FrontFace<PolyCell>> insertions) 
            : base(parentId, id, regime, groups)
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

    
    public override void GiveOrders(LogicWriteKey key)
    {
        var seg = (FrontSegmentAssignment)Parent(key.Data);
        foreach (var kvp in Insertions)
        {
            var group = kvp.Key.Entity(key.Data);
            var destFace = kvp.Value;
            var destCell = destFace.GetNative(key.Data);
            var order = GoToCellGroupOrder.Construct(
                destCell, seg.Regime.Entity(key.Data),
                group, key.Data);
            key.SendMessage(new SetUnitOrderProcedure(group.MakeRef(), order));
        }
    }
    public void DistributeAmong(IEnumerable<FrontSegmentAssignment> segs, LogicWriteKey key)
    {
        foreach (var kvp in Insertions)
        {
            var group = kvp.Key.Entity(key.Data);
            var insertionFace = kvp.Value;
            var seg = segs
                .FirstOrDefault(s => s.Segment.Faces.Contains(insertionFace));
            if (seg == null)
            {
                var groupCell = group.GetCell(key.Data);
                seg = segs.MinBy(s =>
                    s.GetCharacteristicCell(key.Data)
                        .GetCenter()
                        .GetOffsetTo(groupCell.GetCenter(), key.Data).Length());
                insertionFace = seg.Segment.Faces.MinBy(f => 
                    f.GetNative(key.Data).GetCenter()
                        .GetOffsetTo(groupCell.GetCenter(), key.Data).Length());
            }
            Groups.Transfer(group, seg.Insert, key);
        }
        Insertions.Clear();
    }
    
    
    public override void ClearGroupFromData(UnitGroup g, LogicWriteKey key)
    {
        Insertions.Remove(g.MakeRef());
    }

    public override void AddGroupToData(UnitGroup g, LogicWriteKey key)
    {
        var seg = (FrontSegmentAssignment)Parent(key.Data);
        FrontFace<PolyCell> close = seg.Segment.Faces.MinBy(f => f.GetNative(key.Data).GetCenter()
            .GetOffsetTo(g.GetCell(key.Data).GetCenter(), key.Data)
            .Length());
        Insertions[g.MakeRef()] = close;
    }

    public override float GetPowerPointNeed(Data d)
    {
        return 0f;
    }

    public override UnitGroup GetPossibleTransferGroup(LogicWriteKey key)
    {
        if (Insertions.Count == 0) return null;
        return Insertions.First().Key.Entity(key.Data);
    }

    public override PolyCell GetCharacteristicCell(Data d)
    {
        return Insertions.FirstOrDefault().Value.GetNative(d);
    }

    public override void AdjustWithin(LogicWriteKey key)
    {
        var seg = (FrontSegmentAssignment)Parent(key.Data);
        ValidateInsertionPoints(seg, key);
        foreach (var kvp in 
                 Insertions.ToArray())
        {
            var group = kvp.Key.Entity(key.Data);
            var destFace = kvp.Value;
            var destCell = destFace.GetNative(key.Data);
            if (group.GetCell(key.Data) == destCell)
            {
                Groups.Transfer(group, seg.HoldLine, key);
                Insertions.Remove(kvp.Key);
            }
        }
    }
    private void ValidateInsertionPoints(FrontSegmentAssignment seg,
        LogicWriteKey key)
    {
        var insertionCellsByGroupIds = GetInsertionCellsByGroupIds(seg, key.Data);
        foreach (var groupId in Insertions.Keys.ToList())
        {
            ValidateInsertionPoint(groupId, seg, insertionCellsByGroupIds, key.Data);
        }
    }
    private void ValidateInsertionPoint(
        ERef<UnitGroup> groupRef,
        FrontSegmentAssignment seg, 
        Dictionary<Vector2I, FrontFace<PolyCell>> insertionCellsByGroupIds,
        Data d)
    {
        var group = groupRef.Entity(d);
        var old = Insertions[groupRef];
        if (seg.Segment.Faces.Contains(old)) return;

        if (insertionCellsByGroupIds == null)
        {
            var close = insertionCellsByGroupIds
                .MinBy(kvp => kvp.Value.GetNative(d).GetCenter().GetOffsetTo(group.GetCell(d).GetCenter(), d).Length());
            Insertions[groupRef] = close.Value;
        }
        else
        {
            var close = seg.Segment
                .Faces.MinBy(f => f.GetNative(d).GetCenter().GetOffsetTo(group.GetCell(d).GetCenter(), d).Length());
            Insertions[groupRef] = close;
        }
    }
}