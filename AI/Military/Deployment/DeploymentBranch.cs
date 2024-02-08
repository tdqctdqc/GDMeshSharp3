
using System;
using System.Collections.Generic;
using System.Linq;
using Godot;
using MessagePack;

[MessagePack.Union(0, typeof(DeploymentRoot))]
[MessagePack.Union(1, typeof(Theater))]
[MessagePack.Union(2, typeof(FrontSegment))]
public abstract class DeploymentBranch 
    : IPolymorph, IDeploymentNode
{
    public ERef<Regime> Regime { get; private set; }
    public int ParentId { get; private set; }
    public int Id { get; private set; }
    public ReserveAssignment Reserve { get; private set; }
    [SerializationConstructor] protected 
        DeploymentBranch(ERef<Regime> regime, int id,
            int parentId, ReserveAssignment reserve)
    {
        Regime = regime;
        Id = id;
        ParentId = parentId;
        Reserve = reserve;
    }

    public float GetPowerPointsAssigned(Data data)
    {
        return Children().Sum(c => c.GetPowerPointsAssigned(data));
    }
    public DeploymentBranch Parent(DeploymentAi ai, Data d)
    {
        return (DeploymentBranch)ai.GetNode(ParentId);
    }
    public abstract float GetPowerPointNeed(Data d);
    public abstract PolyCell GetCharacteristicCell(Data d);
    public abstract bool PullGroup(DeploymentAi ai, GroupAssignment transferTo, LogicWriteKey key);
    public abstract bool PushGroup(DeploymentAi ai, GroupAssignment transferFrom, LogicWriteKey key);
    public abstract IEnumerable<IDeploymentNode> Children();
    public void AdjustWithin(DeploymentAi ai, LogicWriteKey key)
    {
        foreach (var c in Children())
        {
            c.AdjustWithin(ai, key);
        }
    }
    public void GiveOrders(DeploymentAi ai, LogicWriteKey key)
    {
        foreach (var d in Children())
        {
            d.GiveOrders(ai, key);
        }
    }
    public void Disband(DeploymentAi ai, LogicWriteKey key)
    {
        var oldBranch = (DeploymentTrunk)Parent(ai, key.Data);
        if (oldBranch is null)
        {
            throw new Exception();
        }
        oldBranch.Branches.Remove(this);
        ParentId = -1;
        ai.RemoveNode(Id, key);
    }

    public void SetParent(
        DeploymentAi ai,
        DeploymentTrunk newTrunk,
        LogicWriteKey key)
    {
        if (ParentId != -1)
        {
            var oldTrunk = (DeploymentTrunk)ai.GetNode(ParentId);
            oldTrunk.Branches.Remove(this);
        }
        newTrunk.Branches.Add(this);
        ParentId = newTrunk.Id;
    }

    public IEnumerable<GroupAssignment> GetAssignments()
    {
        var cs = Children();
        return cs.OfType<GroupAssignment>()
            .Union(cs.OfType<DeploymentBranch>()
                .SelectMany(c => c.GetAssignments()));
    }

    public IEnumerable<T> GetChildrenOfType<T>()
        where T : IDeploymentNode
    {
        var cs = Children();
        return cs.OfType<T>()
            .Union(cs.OfType<DeploymentBranch>()
                .SelectMany(c => c.GetChildrenOfType<T>()));
    }

    public Control GetGraphic(Data d)
    {
        var panel = new VBoxContainer();
        panel.CreateLabelAsChild(GetType().Name);
        foreach (var c in Children())
        {
            panel.CreateLabelAsChild($"\t{c.GetType().Name}");
            if (c is GroupAssignment g)
            {
                panel.CreateLabelAsChild($"\t\tGroups: {g.Groups.Count}");
            }
        }

        return panel;
    }

    public abstract Vector2 GetMapPosForDisplay(Data d);

    public void DissolveInto(DeploymentAi ai, DeploymentBranch into,
        LogicWriteKey key)
    {
        foreach (var c in Children())
        {
            c.DissolveInto(ai, into, key);
        }
        var reserves = Reserve.Groups.ToArray();
        for (var i = 0; i < reserves.Length; i++)
        {
            Reserve.Transfer(ai, reserves[i].Entity(key.Data), 
                into.Reserve, key);
        }
        ai.RemoveNode(Id, key);
    }
}