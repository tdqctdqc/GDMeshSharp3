
using System;
using System.Collections.Generic;
using System.Linq;
[MessagePack.Union(0, typeof(HoldLineAssignment))]
public abstract class GroupAssignment : IDeploymentNode
{
    public DeploymentBranch Parent { get; }
    public ERef<Regime> Regime { get; private set; }
    public HashSet<ERef<UnitGroup>> Groups { get; }
    
    protected GroupAssignment(DeploymentBranch parent,
        DeploymentAi ai, LogicWriteKey key)
    {
        Parent = parent;
        Regime = ai.Regime.MakeRef();
        Groups = new HashSet<ERef<UnitGroup>>();
    }

    public void RemoveGroup(DeploymentAi ai, UnitGroup g)
    {
        if (Groups.Contains(g.Id) == false) throw new Exception();
        Groups.Remove(g.Id);
        RemoveGroupFromData(ai, g);
    }
    protected abstract void RemoveGroupFromData(DeploymentAi ai, UnitGroup g);
    
    public void PushGroup(DeploymentAi ai, UnitGroup g, LogicWriteKey key)
    {
        AddGroupToData(ai, g, key.Data);
        if (Groups.Contains(g.Id)) throw new Exception();
        Groups.Add(g.Id);
    }
    protected abstract void AddGroupToData(DeploymentAi ai, UnitGroup g, Data d);
    public abstract float GetPowerPointNeed(Data d);
    public float GetPowerPointsAssigned(Data data)
    {
        return Groups.Select(g => g.Entity(data)).Sum(g => g.GetPowerPoints(data));
    }

    public abstract void GiveOrders(DeploymentAi ai, LogicWriteKey key);
    public abstract UnitGroup PullGroup(DeploymentAi ai, LogicWriteKey key);
    public abstract PolyCell GetCharacteristicCell(Data d);

}