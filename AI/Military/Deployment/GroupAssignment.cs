
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
    public ERef<Alliance> Alliance { get; private set; }
    public HashSet<ERef<Army>> Groups { get; }
    

    protected GroupAssignment(int id, DeploymentBranch parent, ERef<Alliance> alliance, HashSet<ERef<Army>> groups)
    {
        Id = id;
        Parent = parent;
        Alliance = alliance;
        Groups = groups;
    }

    public void RemoveGroup(DeploymentAi ai, Army g)
    {
        if (Groups.Contains(g.MakeRef()) == false) throw new Exception();
        Groups.Remove(g.MakeRef());
        RemoveGroupFromData(ai, g);
    }
    protected abstract void RemoveGroupFromData(DeploymentAi ai, Army g);
    
    public void PushGroup(DeploymentAi ai, Army g, LogicKey key)
    {
        AddGroupToData(ai, g, key.Data);
        if (Groups.Contains(g.MakeRef())) throw new Exception();
        Groups.Add(g.MakeRef());
    }
    protected abstract void AddGroupToData(DeploymentAi ai, Army g, Data d);
    public abstract float GetPowerPointNeed(Data d);
    public float GetPowerPointsAssigned(Data data)
    {
        return Groups.Sum(g => g.Get(data).GetPowerPoints(data));
    }


    public abstract void GiveOrders(DeploymentAi ai, LogicKey key);
    public abstract Army PullGroup(DeploymentAi ai, Func<Army, float> suitability, LogicKey key);
    public abstract float Suitability(Army g, Data d);
    public abstract Cell GetCharacteristicCell(Data d);

}