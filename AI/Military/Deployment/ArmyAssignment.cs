
using System;
using System.Collections.Generic;
using System.Linq;
using Godot;

[MessagePack.Union(0, typeof(FrontlineAssignment))]
public abstract class ArmyAssignment : IDeploymentNode, IIdentifiable
{
    public int Id { get; private set; }
    public DeploymentBranch Parent { get; }
    public ERef<Regime> Regime { get; private set; }
    public HashSet<ERef<Army>> Armies { get; }
    

    protected ArmyAssignment(DeploymentBranch parent, 
        ERef<Regime> regime, HashSet<ERef<Army>> armies, int id)
    {
        Id = id;
        Parent = parent;
        Regime = regime;
        Armies = armies;
    }

    public void RemoveArmy(Army g)
    {
        if (Armies.Contains(g.MakeRef()) == false) throw new Exception();
        Armies.Remove(g.MakeRef());
        RemoveArmyFromData(g);
    }
    protected abstract void RemoveArmyFromData(Army g);
    
    public void PushUnit(Unit u, LogicKey key)
    {
        var min = Armies
            .MinBy(a => a.Get(key.Data).GetPowerPoints(key.Data));
        var proc = new SetUnitArmyProcedure(u.MakeRef(), min);
        key.SendMessage(proc);
    }

    public void PushArmy(Army a, LogicKey key)
    {
        AddArmyToData(a, key.Data);
        if (Armies.Contains(a.MakeRef())) throw new Exception();
        Armies.Add(a.MakeRef());
    }

    public abstract void Draw(MeshBuilder mb, Vector2 relTo, Data d);
    public abstract void MergeToNew(DeploymentRoot newRoot, StrategicContext context, LogicKey key);
    
    

    protected abstract void AddArmyToData(Army g, Data d);
    public abstract float GetPowerPointNeed(Data d);

    public float GetPowerPointsAssigned(Data data)
    {
        return Armies.Sum(g => g.Get(data).GetPowerPoints(data));
    }


    public abstract void GiveOrders(LogicKey key);
    public abstract Unit PullUnit(Func<Unit, float> suitability, LogicKey key);
    public abstract float Suitability(Unit u, Data d);
    public abstract Cell GetCharacteristicCell(Data d);
    public abstract void SetWeights(LogicKey key);

}