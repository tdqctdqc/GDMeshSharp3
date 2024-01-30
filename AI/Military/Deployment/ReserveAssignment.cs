
using System;
using System.Collections.Generic;
using System.Linq;
using MessagePack;

public class ReserveAssignment : DeploymentLeaf
{
    public int CellId { get; private set; }
    public static ReserveAssignment Construct(int parentId,
        ERef<Regime> regime, 
        LogicWriteKey key)
    {
        return new ReserveAssignment(
            key.Data.HostLogicData.DeploymentTreeIds.TakeId(key.Data),
            parentId, regime,
            new UnitGroupManager(),
            -1);
    }
    [SerializationConstructor] private ReserveAssignment(
        int id, int parentId, ERef<Regime> regime, 
        UnitGroupManager groups, int cellId) 
            : base(parentId, id, regime, groups)
    {
        CellId = cellId;
    }
    public override void ClearGroupFromData(UnitGroup g, LogicWriteKey key)
    {
        
    }

    public override void AddGroupToData(UnitGroup g, LogicWriteKey key)
    {
        
    }

    public override float GetPowerPointNeed(Data d)
    {
        return 0f;
    }

    public override void GiveOrders(LogicWriteKey key)
    {
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
    public override UnitGroup GetPossibleTransferGroup(LogicWriteKey key)
    {
        if (Groups.Count() == 0) return null;
        return Groups.Groups.First().Entity(key.Data);
    }

    public override PolyCell GetCharacteristicCell(Data d)
    {
        return PlanetDomainExt.GetPolyCell(CellId, d);
    }

    public override void AdjustWithin(LogicWriteKey key)
    {
        
    }
    public void DistributeAmong(IEnumerable<FrontSegmentAssignment> segs, LogicWriteKey key)
    {
        foreach (var gRef in Groups.Groups.ToArray())
        {
            var group = gRef.Entity(key.Data);
            var groupCell = group.GetCell(key.Data);
            var close = segs
                .MinBy(s =>
                    s.GetCharacteristicCell(key.Data)
                        .GetCenter()
                        .GetOffsetTo(groupCell.GetCenter(), key.Data));
            Groups.Transfer(group, close.Reserve, key);
        }
    }
}