
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
    public int Id { get; private set; }
    public HashSet<DeploymentBranch> SubBranches { get; }
    public HashSet<GroupAssignment> Assignments { get; private set; }
    [SerializationConstructor] protected 
        DeploymentBranch(DeploymentAi ai, LogicWriteKey key)
    {
        Regime = ai.Regime.MakeRef();
        SubBranches = new HashSet<DeploymentBranch>();
        Assignments = new HashSet<GroupAssignment>();
    }

    public float GetPowerPointsAssigned(Data data)
    {
        return Assignments.Sum(a => a.GetPowerPointsAssigned(data))
            + SubBranches.Sum(c => c.GetPowerPointsAssigned(data));
    }
    public abstract float GetPowerPointNeed(Data d);
    public abstract PolyCell GetCharacteristicCell(Data d);

    public UnitGroup PullGroup(DeploymentAi ai, LogicWriteKey key)
    {
        var children = SubBranches
            .Union<IDeploymentNode>(Assignments)
            .OrderByDescending(c => c.GetSatisfiedRatio(key.Data));
        foreach (var c in children)
        {
            var u = c.PullGroup(ai, key);
            if(u != null) return u;
        }
        return null;
    }

    public void PushGroup(DeploymentAi ai, 
        UnitGroup g, LogicWriteKey key)
    {
        var child = SubBranches
                    .Union<IDeploymentNode>(Assignments)
                    .MinBy(c => c.GetSatisfiedRatio(key.Data));
        child.PushGroup(ai, g, key);
    }
    public void AdjustWithin(DeploymentAi ai, LogicWriteKey key)
    {
        
    }
    public void GiveOrders(DeploymentAi ai, LogicWriteKey key)
    {
        
    }
    public void ShiftGroups(DeploymentAi ai, LogicWriteKey key)
    {
        var data = key.Data;
        var eligibleToTakeFrom = SubBranches
            .Union<IDeploymentNode>(Assignments)
            .ToHashSet();
        

        var iter = 0;
        var maxIter = SubBranches.Count * 2 + Assignments.Count();
        var max = maxSatisfied();
        var min = minSatisfied();
        while (iter < maxIter
               && eligibleToTakeFrom.Count > 0
               && max.ratio > min.ratio * 1.5f)
        {
            var g = max.node.PullGroup(ai, key);
            if (g != null)
            {
                min.fa.PushGroup(ai, g, key);
            }
            else
            {
                eligibleToTakeFrom.Remove(max.node);
            }
            
            max = maxSatisfied();
            min = minSatisfied();
            iter++;
        }
        
        foreach (var b in SubBranches)
        {
            b.ShiftGroups(ai, key);
        }
        
        (float ratio, IDeploymentNode node) maxSatisfied()
        {
            var max = eligibleToTakeFrom.MaxBy(fa => fa.GetSatisfiedRatio(data));
            return (max.GetSatisfiedRatio(data), max);
        }
        
        (float ratio, IDeploymentNode fa) minSatisfied()
        {
            var min = SubBranches.MinBy(fa => fa.GetSatisfiedRatio(data));
            return (min.GetSatisfiedRatio(data), min);
        }
    }
    public IEnumerable<GroupAssignment> GetAssignments()
    {
        return Assignments
            .Union(SubBranches
                .SelectMany(c => c.GetAssignments()));
    }

    public IEnumerable<T> GetDescendentsOfType<T>()
        where T : IDeploymentNode
    {
        return Assignments.OfType<T>()
            .Union(SubBranches.OfType<DeploymentBranch>()
                .SelectMany(c => c.GetDescendentsOfType<T>()));
    }

    public Control GetGraphic(Data d)
    {
        var panel = new VBoxContainer();
        panel.CreateLabelAsChild(GetType().Name);
        foreach (var c in Assignments)
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

}