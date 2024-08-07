
using System;
using System.Collections.Generic;
using System.Linq;
using Godot;

[MessagePack.Union(0, typeof(FrontlineAssignment))]
public abstract class ArmyAssignment : IDeploymentNode, 
    IIdentifiable
{
    public int Id { get; private set; }
    public DeploymentBranch Parent { get; }
    public ERef<Alliance> Alliance { get; private set; }
    public HashSet<ERef<Army>> Armies { get; }
    

    protected ArmyAssignment(int id, DeploymentBranch parent, ERef<Alliance> alliance, HashSet<ERef<Army>> armies)
    {
        Id = id;
        Parent = parent;
        Alliance = alliance;
        Armies = armies;
    }

    public void RemoveGroup(DeploymentAi ai, Army g)
    {
        if (Armies.Contains(g.MakeRef()) == false) throw new Exception();
        Armies.Remove(g.MakeRef());
        RemoveGroupFromData(ai, g);
    }
    protected abstract void RemoveGroupFromData(DeploymentAi ai, Army g);
    
    public void PushGroup(Army g, LogicKey key)
    {
        AddGroupToData(g, key.Data);
        if (Armies.Contains(g.MakeRef())) throw new Exception();
        Armies.Add(g.MakeRef());
    }

    public abstract void Draw(MeshBuilder mb, Vector2 relTo, Data d);

    protected abstract void AddGroupToData(Army g, Data d);
    public abstract float GetPowerPointNeed(Data d);
    public float GetPowerPointsAssigned(Data data)
    {
        return Armies.Sum(g => g.Get(data).GetPowerPoints(data));
    }


    public abstract void GiveOrders(LogicKey key);
    public abstract Army PullGroup(Func<Army, float> suitability, LogicKey key);
    public abstract float Suitability(Army g, Data d);
    public abstract Cell GetCharacteristicCell(Data d);
    public abstract void SetWeights(LogicKey key);

}