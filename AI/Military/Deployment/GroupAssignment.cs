
using System;
using System.Collections.Generic;
using System.Linq;
using Godot;

[MessagePack.Union(0, typeof(HoldLineAssignment))]
public abstract class GroupAssignment : IDeploymentNode, IIdentifiable
{
    public int Id { get; private set; }
    public DeploymentBranch Parent { get; }
    public Regime Regime { get; private set; }
    public HashSet<UnitGroup> Groups { get; }
    
    protected GroupAssignment(DeploymentBranch parent,
        DeploymentAi ai, LogicWriteKey key)
    {
        Id = ai.IdDispenser.TakeId();
        Parent = parent;
        Regime = ai.Regime;
        Groups = new HashSet<UnitGroup>();
    }

    public void RemoveGroup(DeploymentAi ai, UnitGroup g)
    {
        if (Groups.Contains(g) == false) throw new Exception();
        Groups.Remove(g);
        RemoveGroupFromData(ai, g);
    }
    protected abstract void RemoveGroupFromData(DeploymentAi ai, UnitGroup g);
    
    public void PushGroup(DeploymentAi ai, UnitGroup g, LogicWriteKey key)
    {
        AddGroupToData(ai, g, key.Data);
        if (Groups.Contains(g)) throw new Exception();
        Groups.Add(g);
    }
    protected abstract void AddGroupToData(DeploymentAi ai, UnitGroup g, Data d);
    public abstract float GetPowerPointNeed(Data d);
    public float GetPowerPointsAssigned(Data data)
    {
        return Groups.Sum(g => g.GetPowerPoints(data));
    }


    public abstract void GiveOrders(DeploymentAi ai, LogicWriteKey key);
    public abstract UnitGroup PullGroup(DeploymentAi ai, Func<UnitGroup, float> suitability, LogicWriteKey key);
    public abstract float Suitability(UnitGroup g, Data d);
    public abstract Cell GetCharacteristicCell(Data d);

}