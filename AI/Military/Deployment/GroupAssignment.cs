
using System;
using System.Collections.Generic;
using System.Linq;
[MessagePack.Union(0, typeof(ReserveAssignment))]
[MessagePack.Union(1, typeof(HoldLineAssignment))]
[MessagePack.Union(2, typeof(InsertionAssignment))]
public abstract class GroupAssignment : IDeploymentNode
{
    public int ParentId { get; }
    public int Id { get; }
    public ERef<Regime> Regime { get; private set; }
    public UnitGroupManager Groups { get; }
    
    protected GroupAssignment(int parentId, int id, ERef<Regime> regime, UnitGroupManager groups)
    {
        ParentId = parentId;
        Id = id;
        Regime = regime;
        Groups = groups;
    }
    
    public abstract void ClearGroupFromData(DeploymentAi ai, UnitGroup g, LogicWriteKey key);
    public abstract void AddGroupToData(DeploymentAi ai, UnitGroup g, LogicWriteKey key);
    public abstract float GetPowerPointNeed(Data d);
    public DeploymentBranch Parent(DeploymentAi ai, Data d)
    {
        return (DeploymentBranch)ai.GetNode(ParentId);
    }
    IEnumerable<IDeploymentNode> IDeploymentNode.Children() => Enumerable.Empty<IDeploymentNode>();
    public float GetPowerPointsAssigned(Data data)
    {
        return Groups.Get(data).Sum(g => g.GetPowerPoints(data));
    }

    public void Disband(DeploymentAi ai, LogicWriteKey key)
    {
        Groups.Disband(key);
        ai.RemoveNode(Id, key);
    }
    public abstract void AdjustWithin(DeploymentAi ai, LogicWriteKey key);
    public abstract void GiveOrders(DeploymentAi ai, LogicWriteKey key);
    public abstract bool PullGroup(DeploymentAi ai, GroupAssignment transferTo, LogicWriteKey key);
    public abstract PolyCell GetCharacteristicCell(Data d);
}