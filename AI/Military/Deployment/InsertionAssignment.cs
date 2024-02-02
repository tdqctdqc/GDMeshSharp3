
using System;
using System.Collections.Generic;
using System.Linq;
using Godot;
using MessagePack;

public class InsertionAssignment : GroupAssignment
{
    public Dictionary<ERef<UnitGroup>, FrontFace<PolyCell>> Insertions { get; private set; }
    public static InsertionAssignment Construct(
        DeploymentAi ai,
        int parentId,
        ERef<Regime> regime, LogicWriteKey key)
    {
        var id = ai.DeploymentTreeIds.TakeId(key.Data);
        var a = new InsertionAssignment(
            id,
            parentId, regime, UnitGroupManager.Construct(regime, id),
            new Dictionary<ERef<UnitGroup>, FrontFace<PolyCell>>());
        return a;
    }

    [SerializationConstructor] private InsertionAssignment(
        int id, int parentId, ERef<Regime> regime, UnitGroupManager groups,
        Dictionary<ERef<UnitGroup>, FrontFace<PolyCell>> insertions) 
            : base(parentId, id, regime, groups)
    {
        Insertions = insertions;
    }

    private Dictionary<Vector2I, FrontFace<PolyCell>> 
        GetInsertionCellsByGroupIds(FrontSegment seg, 
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
            seg.Frontline.Faces.First());
        for (var i = 0; i < lineGroupIds.Count - 1; i++)
        {
            var prevGroupId = lineGroupIds[i];
            var nextGroupId = lineGroupIds[i + 1];
            var lastFaceOfPrev = seg.HoldLine.FacesByGroupId[prevGroupId].Last();
            insertionCellsByGroupIds.Add(new Vector2I(prevGroupId, nextGroupId),
                lastFaceOfPrev);
        }
        insertionCellsByGroupIds.Add(new Vector2I(lineGroupIds.Last(), -1),
            seg.Frontline.Faces.Last());
        return insertionCellsByGroupIds;
    }

    
    public override void GiveOrders(DeploymentAi ai, LogicWriteKey key)
    {
        var seg = (FrontSegment)Parent(ai, key.Data);
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
    public void DissolveInto(DeploymentAi ai, IEnumerable<FrontSegment> segs, LogicWriteKey key)
    {
        foreach (var kvp in Insertions)
        {
            var group = kvp.Key.Entity(key.Data);
            var insertionFace = kvp.Value;
            var seg = segs
                .FirstOrDefault(s => s.Frontline.Faces.Contains(insertionFace));
            if (seg == null)
            {
                var groupCell = group.GetCell(key.Data);
                seg = segs.MinBy(s =>
                    s.GetCharacteristicCell(key.Data)
                        .GetCenter()
                        .GetOffsetTo(groupCell.GetCenter(), key.Data).Length());
                insertionFace = seg.Frontline.Faces.MinBy(f => 
                    f.GetNative(key.Data).GetCenter()
                        .GetOffsetTo(groupCell.GetCenter(), key.Data).Length());
            }
            Groups.Transfer(ai, group, seg.Insert, key);
        }
        Insertions.Clear();
    }
    
    
    public override void ClearGroupFromData(DeploymentAi ai, UnitGroup g, LogicWriteKey key)
    {
        Insertions.Remove(g.MakeRef());
    }

    public override void AddGroup(DeploymentAi ai, UnitGroup g, LogicWriteKey key)
    {
        var seg = (FrontSegment)Parent(ai, key.Data);
        FrontFace<PolyCell> close = seg.Frontline.Faces.MinBy(f => f.GetNative(key.Data).GetCenter()
            .GetOffsetTo(g.GetCell(key.Data).GetCenter(), key.Data)
            .Length());
        Insertions[g.MakeRef()] = close;
        Groups.Add(ai, g, key);
    }

    public override float GetPowerPointNeed(Data d)
    {
        var ai = d.HostLogicData.RegimeAis[Regime.Entity(d)]
            .Military.Deployment;
        var seg = (FrontSegment)Parent(ai, d);
        var holdLine = seg.HoldLine;
        var need = holdLine.GetPowerPointNeed(d) - holdLine.GetPowerPointsAssigned(d);
        need = Mathf.Max(0f, need);
        return need;
    }

    public override bool PullGroup(DeploymentAi ai, GroupAssignment transferTo,
        LogicWriteKey key)
    {
        if (Insertions.Count == 0) return false;
        var g = Insertions.First().Key.Entity(key.Data);
        Groups.Transfer(ai, g, transferTo, key);
        return true;

    }
    public override PolyCell GetCharacteristicCell(Data d)
    {
        return Insertions.FirstOrDefault().Value.GetNative(d);
    }

    public override void AdjustWithin(DeploymentAi ai, LogicWriteKey key)
    {
        var seg = (FrontSegment)Parent(ai, key.Data);
        ValidateInsertionPoints(seg, key);
        foreach (var kvp in 
                 Insertions.ToArray())
        {
            var group = kvp.Key.Entity(key.Data);
            var destFace = kvp.Value;
            var destCell = destFace.GetNative(key.Data);
            if (group.GetCell(key.Data) == destCell)
            {
                Groups.Transfer(ai, group, seg.HoldLine, key);
                Insertions.Remove(kvp.Key);
            }
        }
    }
    private void ValidateInsertionPoints(FrontSegment seg,
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
        FrontSegment seg, 
        Dictionary<Vector2I, FrontFace<PolyCell>> insertionCellsByGroupIds,
        Data d)
    {
        var group = groupRef.Entity(d);
        var old = Insertions[groupRef];
        if (seg.Frontline.Faces.Contains(old)) return;

        if (insertionCellsByGroupIds == null)
        {
            var close = insertionCellsByGroupIds
                .MinBy(kvp => kvp.Value.GetNative(d).GetCenter().GetOffsetTo(group.GetCell(d).GetCenter(), d).Length());
            Insertions[groupRef] = close.Value;
        }
        else
        {
            var close = seg.Frontline
                .Faces.MinBy(f => f.GetNative(d).GetCenter().GetOffsetTo(group.GetCell(d).GetCenter(), d).Length());
            Insertions[groupRef] = close;
        }
    }
}