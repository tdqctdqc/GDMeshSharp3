
using System;
using System.Collections.Generic;
using System.Linq;
using Godot;

[MessagePack.Union(0, typeof(FrontlineAssignment))]
public abstract class GroupAssignment : IDeploymentNode, 
    IIdentifiable
{
    public int Id { get; private set; }
    public DeploymentBranch Parent { get; }
    public Alliance Alliance { get; private set; }
    public HashSet<Army> Groups { get; }
    
    protected GroupAssignment(DeploymentBranch parent,
        DeploymentAi ai, LogicKey key)
    {
        Id = ai.IdDispenser.TakeId();
        Parent = parent;
        Alliance = ai.Alliance;
        Groups = new HashSet<Army>();
    }

    public void RemoveGroup(DeploymentAi ai, Army g)
    {
        if (Groups.Contains(g) == false) throw new Exception();
        Groups.Remove(g);
        RemoveGroupFromData(ai, g);
    }
    protected abstract void RemoveGroupFromData(DeploymentAi ai, Army g);
    
    public void PushGroup(DeploymentAi ai, Army g, LogicKey key)
    {
        AddGroupToData(ai, g, key.Data);
        if (Groups.Contains(g)) throw new Exception();
        Groups.Add(g);
    }
    protected abstract void AddGroupToData(DeploymentAi ai, Army g, Data d);
    public abstract float GetPowerPointNeed(Data d);
    public float GetPowerPointsAssigned(Data data)
    {
        return Groups.Sum(g => g.GetPowerPoints(data));
    }


    public abstract void GiveOrders(DeploymentAi ai, LogicKey key);
    public abstract Army PullGroup(DeploymentAi ai, Func<Army, float> suitability, LogicKey key);
    public abstract float Suitability(Army g, Data d);
    public abstract Cell GetCharacteristicCell(Data d);

}