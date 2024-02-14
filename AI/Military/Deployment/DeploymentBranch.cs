
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

    public float GetPowerPointNeed(Data d)
    {
        return SubBranches.Sum(s => s.GetPowerPointNeed(d))
               + Assignments.Sum(a => a.GetPowerPointNeed(d));
    }
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
        if (child == null)
        {
            throw new Exception("no children " + this.GetType());
        }
        child.PushGroup(ai, g, key);
    }
    public void GiveOrders(DeploymentAi ai, LogicWriteKey key)
    {
        foreach (var ga in Assignments)
        {
            ga.GiveOrders(ai, key);
        }

        foreach (var d in SubBranches)
        {
            d.GiveOrders(ai, key);
        }
    }
    public void ShiftGroups(DeploymentAi ai, LogicWriteKey key)
    {
        var data = key.Data;
        var children = SubBranches
            .Union<IDeploymentNode>(Assignments)
            .ToHashSet();
        var eligibleToTakeFrom = children.ToHashSet();
        var eligibleToGiveTo = children
            .Where(c => c.GetPowerPointNeed(data) > 0f).ToHashSet();
        var iter = 0;
        var shuffleCount = Assignments.OfType<UnoccupiedAssignment>()
            .FirstOrDefault() is UnoccupiedAssignment uo
            ? uo.Groups.Count
            : 0;
        var maxIter = (SubBranches.Count
                       + Assignments.Count) * 2 + shuffleCount;
        
        while (iter < maxIter
               && eligibleToTakeFrom.Count > 0
               && eligibleToGiveTo.Count > 0)
        {
            var max = maxSatisfied();
            var min = minSatisfied();
            if (max.node == null
                || min.node == null
                || max.ratio < min.ratio * 1.5f)
            {
                break;
            }
            var g = max.node.PullGroup(ai, key);
            if (g != null)
            {
                min.node.PushGroup(ai, g, key);
            }
            else
            {
                eligibleToTakeFrom.Remove(max.node);
            }
            
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
        
        (float ratio, IDeploymentNode node) minSatisfied()
        {
            var min = eligibleToGiveTo
                .MinBy(fa => fa.GetSatisfiedRatio(data));
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
            panel.CreateLabelAsChild($"\aAssigned {c.GetPowerPointsAssigned(d)}");
            panel.CreateLabelAsChild($"\tNeeded {c.GetPowerPointNeed(d)}");

        }

        return panel;
    }

    public abstract Vector2 GetMapPosForDisplay(Data d);

}