
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
        var assignments =
            GetDescendentAssignments().ToHashSet();
        var stratMove = key.Data.Models.MoveTypes.StrategicMove;
        var alliance = Regime.Entity(key.Data)
            .GetAlliance(key.Data);
        var graph = new Graph<GroupAssignment, float>();
        foreach (var a in assignments)
        {
            graph.AddNode(a);
        }
        foreach (var a1 in assignments)
        {
            var cell1 = a1.GetCharacteristicCell(key.Data);
            foreach (var a2 in assignments)
            {
                if (graph.HasEdge(a1, a2)) continue;
                var cell2 = a2.GetCharacteristicCell(key.Data);
                var path = PathFinder.FindPath(stratMove,
                    alliance, cell1, cell2, key.Data);
                var cost = 0f;
                for (var i = 0; i < path.Count - 1; i++)
                {
                    cost += stratMove.EdgeCost(cell1, cell2, key.Data);
                }
                graph.AddEdge(a1, a2, cost);
            }
        }

        var unoccupiedAssigns = assignments.OfType<UnoccupiedAssignment>();
        var shuffleCount = unoccupiedAssigns.Any()
            ? unoccupiedAssigns.Sum(u => u.Groups.Count)
            : 0;
        var maxIter = SubBranches.Count
                       + Assignments.Count + shuffleCount;

        var noGroupsToTake = new HashSet<GroupAssignment>();

        var iter = 0;

        while (iter < maxIter)
        {
            foreach (var a in assignments)
            {
                iter++;
                var ratio = a.GetSatisfiedRatio(key.Data);
                var neighborsToTakeFrom = graph
                    .GetNeighbors(a)
                    .Where(n => n.GetSatisfiedRatio(key.Data) > 1.5f * ratio
                                && noGroupsToTake.Contains(n) == false)
                    .OrderBy(n => graph.GetEdge(a, n));
                foreach (var n in neighborsToTakeFrom)
                {
                    var take = n.PullGroup(ai, key);
                    if (take == null)
                    {
                        noGroupsToTake.Add(n);
                    }
                    else
                    {
                        a.PushGroup(ai, take, key);
                        break;
                    }
                }
            }
        }  
        
        foreach (var b in SubBranches)
        {
            b.ShiftGroups(ai, key);
        }
    }

    public IEnumerable<GroupAssignment> GetDescendentAssignments()
    {
        return Assignments.Union(SubBranches.SelectMany(s => s.GetDescendentAssignments()));
    }
    public IEnumerable<T> GetDescendentAssignmentsOfType<T>()
        where T : IDeploymentNode
    {
        return Assignments.OfType<T>()
            .Union(SubBranches
                .SelectMany(c => c.GetDescendentAssignmentsOfType<T>()));
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