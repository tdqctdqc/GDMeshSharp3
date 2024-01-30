
using System;
using System.Collections.Generic;
using System.Linq;
using Godot;
using MessagePack;

[MessagePack.Union(0, typeof(Front))]
[MessagePack.Union(1, typeof(Theater))]
[MessagePack.Union(2, typeof(FrontSegmentAssignment))]
public abstract class DeploymentBranch 
    : IPolymorph, 
    IDeploymentNode
{
    public ERef<Regime> Regime { get; private set; }
    public int ParentId { get; private set; }
    public int Id { get; private set; }
    public ReserveAssignment Reserve { get; private set; }
    [SerializationConstructor] protected 
        DeploymentBranch(ERef<Regime> regime, int id)
    {
        Regime = regime;
        Id = id;
    }

    public float GetPowerPointsAssigned(Data data)
    {
        return Children().Sum(c => c.GetPowerPointsAssigned(data));
    }
    public DeploymentBranch Parent(Data d)
    {
        return (DeploymentBranch)d.HostLogicData.RegimeAis.Dic[Regime.Entity(d)]
            .Military.Deployment.GetNode(ParentId);
    }
    public abstract float GetPowerPointNeed(Data d);
    public abstract PolyCell GetCharacteristicCell(Data d);
    public abstract UnitGroup GetPossibleTransferGroup(LogicWriteKey key);
    public abstract IEnumerable<IDeploymentNode> Children();
    public abstract void DissolveInto(IEnumerable<DeploymentBranch> into, LogicWriteKey key);
    public abstract void AdjustWithin(LogicWriteKey key);

    public void GiveOrders(LogicWriteKey key)
    {
        foreach (var d in Children())
        {
            d.GiveOrders(key);
        }
    }
    public void Disband(LogicWriteKey key)
    {
        foreach (var n in Children())
        {
            n.Disband(key);
        }
        Orphan(key);
        key.Data.HostLogicData.RegimeAis.Dic[Regime.Entity(key.Data)]
            .Military.Deployment.RemoveNode(Id, key);
    }

    public void SetParent(CompoundDeploymentBranch newBranch,
        LogicWriteKey key)
    {
        var oldBranch = Parent(key.Data);
        if (oldBranch is not null)
        {
            throw new Exception();
        }
        newBranch.Assignments.Add(this);
        ParentId = newBranch.Id;
    }

    public void Orphan(LogicWriteKey key)
    {
        var oldBranch = (CompoundDeploymentBranch)Parent(key.Data);
        if (oldBranch is null)
        {
            throw new Exception();
        }
        oldBranch.Assignments.Remove(this);
    }
}