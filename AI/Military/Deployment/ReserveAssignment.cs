
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
            new HashSet<ERef<UnitGroup>>(),
            -1, 1.5f);
        return a;
    }
    [SerializationConstructor] private ReserveAssignment(
        int id, int parentId, ERef<Regime> regime, 
        HashSet<ERef<UnitGroup>> groups, int cellId, float reserveThreshold) 
            : base(parentId, id, regime, groups)
    {
        ReserveThreshold = reserveThreshold;
        CellId = cellId;
    }
    protected override void RemoveGroupFromData(DeploymentAi ai, UnitGroup g)
    {
    }

    protected override bool TryAddGroupToData(DeploymentAi ai, UnitGroup g, Data d)
    {
        return true;
    }

    public override float GetPowerPointNeed(Data d)
    {
        return 0f;
    }

    public override void GiveOrders(DeploymentAi ai, LogicWriteKey key)
    {
        if (CellId == -1) return;
        var cell = PlanetDomainExt.GetPolyCell(CellId, key.Data);
        foreach (var gRef in Groups)
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
        var g = Groups.First().Entity(key.Data);
        Transfer(ai, g, transferTo, key);
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
            var g = Groups.First().Entity(key.Data);
            Transfer(ai, g, min.sibling, key);
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
    public void Distribute(DeploymentAi ai, IEnumerable<FrontSegment> segs, LogicWriteKey key)
    {
        foreach (var gRef in Groups.ToArray())
        {
            var group = gRef.Entity(key.Data);
            var groupCell = group.GetCell(key.Data);
            var close = segs
                .MinBy(s =>
                    s.GetCharacteristicCell(key.Data)
                        .GetCenter()
                        .GetOffsetTo(groupCell.GetCenter(), key.Data).Length());
            Transfer(ai, group, close.Reserve, key);
        }
    }
}