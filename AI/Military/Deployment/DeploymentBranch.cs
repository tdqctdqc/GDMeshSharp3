
using System;
using System.Collections.Generic;
using System.Linq;
using Godot;
using MessagePack;

[MessagePack.Union(0, typeof(DeploymentRoot))]
[MessagePack.Union(1, typeof(Theater))]
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

    public UnitGroup PullGroup(DeploymentAi ai, 
        Func<UnitGroup, float> suitability, 
        LogicWriteKey key)
    {
        var children = SubBranches
            .Union<IDeploymentNode>(Assignments)
            .OrderByDescending(c => c.GetSatisfiedRatio(key.Data));
        foreach (var c in children)
        {
            var u = c.PullGroup(ai, suitability, key);
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
        var d = key.Data;
        var assignments =
            GetDescendentAssignments()
                .ToHashSet();
        var needs = assignments.ToDictionary(a => a,
            a => a.GetPowerPointNeed(key.Data));
        
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
                
                var cost = 0f;
                var path = d.Context.PathCache.GetOrAdd((stratMove,
                    alliance, cell1, cell2));
                if (path == null)
                {
                    cost = Mathf.Inf;
                }
                else
                {
                    for (var i = 0; i < path.Count - 1; i++)
                    {
                        cost += stratMove.EdgeCost(cell1, cell2, key.Data);
                    }
                }
                
                graph.AddEdge(a1, a2, cost);
            }
        }
        
        var noGroupsToTake = new HashSet<GroupAssignment>();
        var wantGroups = assignments
            .Where(g => needs[g] > 0f)
            .ToHashSet();
        var noNeedAssignments = assignments
            .Except(wantGroups).ToHashSet();
        var maxIter = (wantGroups.Count 
            + noNeedAssignments.Sum(g => g.Groups.Count));
        
        var iter = 0;
        
        while (wantGroups.Count > 0
            && iter < maxIter)
        {

            foreach (var a in wantGroups)
            {
                iter++;
                var need = needs[a];
                var ratio = a.GetPowerPointsAssigned(key.Data) / need;

                bool found = false;
                foreach (var noNeed in noNeedAssignments.OrderBy(n => graph.GetEdge(a, n)))
                {
                    if (eligibleToTakeFrom(noNeed, ratio)
                        && noNeed.PullGroup(ai, 
                            g => a.Suitability(g, key.Data), key)
                        is UnitGroup g)
                    {
                        a.PushGroup(ai, g, key);
                        found = true;
                        break;
                    }
                }

                if (found) break;
                 
                foreach (var ga in assignments.Except(noNeedAssignments)
                             .OrderBy(n => graph.GetEdge(a, n)))
                {
                    if (eligibleToTakeFrom(ga, ratio)
                        && ga.PullGroup(ai, 
                                g => a.Suitability(g, key.Data), key)
                            is UnitGroup g)
                    {
                        a.PushGroup(ai, g, key);
                        found = true;
                        break;
                    }
                }
            }
        }

        Assignments.RemoveWhere(b => b.Groups.Count == 0);
        
        foreach (var b in SubBranches)
        {
            b.ShiftGroups(ai, key);
        }

        SubBranches.RemoveWhere(b => b.SubBranches.Count == 0 && b.Assignments.Count == 0);
        
        bool eligibleToTakeFrom(GroupAssignment assgn, float ratio)
        {
            if (noGroupsToTake.Contains(assgn)) return false;
            if (assgn.Groups.Count == 0) return false;
            var need = needs[assgn];
            if (need == 0f) return true;
            var assgnRatio = assgn.GetPowerPointsAssigned(key.Data) / need;
            if (assgnRatio > 1.5f * ratio) return true;
            return false;
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

    public abstract Vector2 GetMapPosForDisplay(Data d);

}