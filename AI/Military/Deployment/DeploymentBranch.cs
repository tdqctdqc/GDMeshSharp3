
using System;
using System.Collections.Generic;
using System.Linq;
using Godot;
using MessagePack;

[MessagePack.Union(0, typeof(DeploymentRoot))]
[MessagePack.Union(1, typeof(TheaterBranch))]
public abstract class DeploymentBranch 
    : IPolymorph, IDeploymentNode
{
    public Alliance Alliance { get; private set; }
    public int Id { get; private set; }
    public HashSet<DeploymentBranch> SubBranches { get; }
    public HashSet<GroupAssignment> Assignments { get; private set; }
    [SerializationConstructor] protected 
        DeploymentBranch(Alliance alliance, LogicWriteKey key)
    {
        Alliance = alliance;
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
    public abstract Cell GetCharacteristicCell(Data d);

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
                .OrderBy(a => a.GetSatisfiedRatio(key.Data)).ToList();
        var needs = assignments
            .ToDictionary(a => a,
            a => a.GetPowerPointNeed(key.Data));
        var numWant = needs
            .Where(kvp => kvp.Value > 0f)
            .Count();
        if (numWant == 0) return;
        
        
        var stratMove = key.Data.Models.MoveTypes.StrategicMove;
        var alliance = Alliance;
        var distCosts = new Dictionary<Vector2I, float>();
        
        foreach (var a1 in assignments)
        {
            var cell1 = a1.GetCharacteristicCell(key.Data);
            foreach (var a2 in assignments)
            {
                if (a2.Id <= a1.Id) continue;
                var idKey = a1.GetIdEdgeKey(a2);
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
                distCosts.Add(idKey, cost);
            }
        }

        
        
        var maxIter = assignments
            .Sum(a => a.Groups.Count) 
             / 2 + 1;
        
        var iter = 0;
        
        while (iter < maxIter)
        {
            for (var i = 0; i < assignments.Count; i++)
            {
                var a = assignments[i];
                iter++;
                var need = needs[a];
                if (need == 0) continue;
                var ratio = a.GetPowerPointsAssigned(key.Data) / need;
                
                for (var j = assignments.Count - 1; j >= 0; j--)
                {
                    if (i == j) continue;
                    var a2 = assignments[j];
                    if (eligibleToTakeFrom(a2, ratio)
                        && a2.PullGroup(ai, 
                                g => a.Suitability(g, key.Data), key)
                            is UnitGroup g)
                    {
                        a.PushGroup(ai, g, key);
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