
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
    public HashSet<ERef<UnitGroup>> Groups { get; }
    
    protected GroupAssignment(int parentId, int id, 
        ERef<Regime> regime, HashSet<ERef<UnitGroup>> groups)
    {
        ParentId = parentId;
        Id = id;
        Regime = regime;
        Groups = groups;
    }

    public void RemoveGroup(DeploymentAi ai, UnitGroup g)
    {
        if (Groups.Contains(g.Id) == false) throw new Exception();
        Groups.Remove(g.Id);
        ai.GroupAssignments.Remove(g.MakeRef());
        RemoveGroupFromData(ai, g);
    }
    protected abstract void RemoveGroupFromData(DeploymentAi ai, UnitGroup g);
    public void AddUnassigned(DeploymentAi ai, UnitGroup g, Data d)
    {
        AddGroup(ai, g, d);
    }
    public void AddGroup(DeploymentAi ai, UnitGroup g, Data d)
    {
        var added = TryAddGroupToData(ai, g, d);
        if (added == false) return;
        if (Groups.Contains(g.Id)) throw new Exception();
        Groups.Add(g.Id);
        ai.GroupAssignments.Add(g.MakeRef(), this);
    }
    //returns false if redirected to another group
    protected abstract bool TryAddGroupToData(DeploymentAi ai, UnitGroup g, Data d);
    
    public void Transfer(
        DeploymentAi ai,
        UnitGroup g, 
        GroupAssignment to, 
        LogicWriteKey key)
    {
        if (Groups.Contains(g.Id) == false) throw new Exception();
        RemoveGroup(ai, g);
        to.AddGroup(ai, g, key.Data);
    }
    
    public abstract float GetPowerPointNeed(Data d);
    public DeploymentBranch Parent(DeploymentAi ai, Data d)
    {
        return (DeploymentBranch)ai.GetNode(ParentId);
    }
    IEnumerable<IDeploymentNode> IDeploymentNode.Children() => Enumerable.Empty<IDeploymentNode>();
    public float GetPowerPointsAssigned(Data data)
    {
        return Groups.Select(g => g.Entity(data)).Sum(g => g.GetPowerPoints(data));
    }

    public void Disband(DeploymentAi ai, LogicWriteKey key)
    {
        ai.RemoveNode(Id, key);
    }
    public abstract void AdjustWithin(DeploymentAi ai, LogicWriteKey key);
    public abstract void GiveOrders(DeploymentAi ai, LogicWriteKey key);
    public abstract bool PullGroup(DeploymentAi ai, GroupAssignment transferTo, LogicWriteKey key);
    public abstract PolyCell GetCharacteristicCell(Data d);
}