
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using MessagePack;

public class ReserveAssignment : GroupAssignment
{
    public float ReserveThreshold { get; private set; }
    public int CellId { get; private set; }
    public static ReserveAssignment Construct(
        DeploymentAi ai,
        int parentId,
        ERef<Regime> regime, 
        Data d)
    {
        var id = ai.DeploymentTreeIds.TakeId(d); 
        var a = new ReserveAssignment(
            id,
            parentId, regime,
            UnitGroupManager.Construct(regime, id),
            -1, 1.5f);
        return a;
    }
    [SerializationConstructor] private ReserveAssignment(
        int id, int parentId, ERef<Regime> regime, 
        UnitGroupManager groups, int cellId, float reserveThreshold) 
            : base(parentId, id, regime, groups)
    {
        ReserveThreshold = reserveThreshold;
        CellId = cellId;
    }
    public override void ClearGroupFromData(DeploymentAi ai, UnitGroup g, LogicWriteKey key)
    {
        
    }

    public override void AddGroup(DeploymentAi ai, UnitGroup g, LogicWriteKey key)
    {
        Groups.Add(ai, g, key);
    }

    public override float GetPowerPointNeed(Data d)
    {
        return 0f;
    }

    public override void GiveOrders(DeploymentAi ai, LogicWriteKey key)
    {
        if (CellId == -1) return;
        var cell = PlanetDomainExt.GetPolyCell(CellId, key.Data);
        foreach (var gRef in Groups.Groups)
        {
            var group = gRef.Entity(key.Data);
            if (group.GetCell(key.Data) != cell)
            {
                var order = GoToCellGroupOrder.Construct(cell,
                    Regime.Entity(key.Data), group, key.Data);
                var proc = new SetUnitOrderProcedure(gRef, order);
                key.SendMessage(proc);
            }
        }
    }
    public override bool PullGroup(DeploymentAi ai, GroupAssignment transferTo,
        LogicWriteKey key)
    {
        if (Groups.Count() == 0) return false;
        var g = Groups.Groups.First().Entity(key.Data);
        Groups.Transfer(ai, g, transferTo, key);
        return true;
    }


    public override PolyCell GetCharacteristicCell(Data d)
    {
        return PlanetDomainExt.GetPolyCell(CellId, d);
    }

    public override void AdjustWithin(DeploymentAi ai, LogicWriteKey key)
    {
        var parent = Parent(ai, key.Data);

        var min = getMin();
        while (min.sibling != null 
               && Groups.Count() > 0
               && min.ratio < ReserveThreshold)
        {
            var g = Groups.Groups.First().Entity(key.Data);
            Groups.Transfer(ai, g, min.sibling, key);
            min = getMin();
        }

        (GroupAssignment sibling, float ratio) getMin()
        {
            var siblings = parent.Children()
                .OfType<GroupAssignment>()
                .Where(s => s != this)
                .Select(s => (s, s.GetSatisfiedRatio(key.Data)));
            if (siblings.Count() == 0) return (null, 1f);
            return siblings.MinBy(v => v.Item2);
        }
    }
    public void DissolveInto(DeploymentAi ai, IEnumerable<FrontSegment> segs, LogicWriteKey key)
    {
        foreach (var gRef in Groups.Groups.ToArray())
        {
            var group = gRef.Entity(key.Data);
            var groupCell = group.GetCell(key.Data);
            var close = segs
                .MinBy(s =>
                    s.GetCharacteristicCell(key.Data)
                        .GetCenter()
                        .GetOffsetTo(groupCell.GetCenter(), key.Data).Length());
            Groups.Transfer(ai, group, close.Reserve, key);
        }
    }
}