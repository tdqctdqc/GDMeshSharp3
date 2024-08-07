
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
    public ERef<Alliance> Alliance { get; private set; }
    public int Id { get; private set; }
    public HashSet<DeploymentBranch> SubBranches { get; }
    public HashSet<ArmyAssignment> Assignments { get; private set; }


    [SerializationConstructor] protected DeploymentBranch(ERef<Alliance> alliance, int id, HashSet<DeploymentBranch> subBranches, HashSet<ArmyAssignment> assignments)
    {
        Alliance = alliance;
        Id = id;
        SubBranches = subBranches;
        Assignments = assignments;
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

    public void SetWeights(LogicKey key)
    {
        foreach (var armyAssignment in Assignments)
        {
            armyAssignment.SetWeights(key);
        }
        foreach (var deploymentBranch in SubBranches)
        {
            deploymentBranch.SetWeights(key);
        }
    }

    public abstract Cell GetCharacteristicCell(Data d);

    public Army PullGroup(Func<Army, float> suitability, 
        LogicKey key)
    {
        var children = SubBranches
            .Union<IDeploymentNode>(Assignments)
            .OrderByDescending(c => c.GetSatisfiedRatio(key.Data));
        foreach (var c in children)
        {
            var u = c.PullGroup(suitability, key);
            if(u != null) return u;
        }
        return null;
    }

    public void PushGroup(Army g, LogicKey key)
    {
        var child = SubBranches
                    .Union<IDeploymentNode>(Assignments)
                    .MinBy(c => c.GetSatisfiedRatio(key.Data));
        if (child == null)
        {
            throw new Exception("no children " + this.GetType());
        }
        child.PushGroup(g, key);
    }

    public abstract void Draw(MeshBuilder mb, Vector2 relTo, Data d);

    public void GiveOrders(LogicKey key)
    {
        foreach (var ga in Assignments)
        {
            ga.GiveOrders(key);
        }

        foreach (var d in SubBranches)
        {
            d.GiveOrders(key);
        }
    }
    public void ShiftGroups(LogicKey key)
    {
        var d = key.Data;
        var assignments =
            GetDescendentAssignments()
            .OrderBy(a => a.GetSatisfiedRatio(key.Data))
            .ToList();
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
                var path = d.Context.FriendlyPathCache.GetOrAdd((stratMove,
                    alliance.Get(d), cell1, cell2));
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
            .Sum(a => a.Armies.Count) + 1;
        
        var iter = 0;
        
        while (iter < maxIter)
        {
            for (var i = 0; i < assignments.Count; i++)
            {
                var a = assignments[i];
                iter++;
                var need = needs[a];
                if (need <= 0) continue;
                var ratio = a.GetPowerPointsAssigned(key.Data) / need;
                
                for (var j = assignments.Count - 1; j >= 0; j--)
                {
                    if (i == j) continue;
                    var a2 = assignments[j];
                    if (eligibleToTakeFrom(a2, ratio)
                        && a2.PullGroup(g => a.Suitability(g, key.Data), key)
                            is Army g)
                    {
                        a.PushGroup(g, key);
                        // break;
                    }
                }
            }
        }

        Assignments.RemoveWhere(b => b.Armies.Count == 0);
        
        foreach (var b in SubBranches)
        {
            b.ShiftGroups(key);
        }

        SubBranches.RemoveWhere(b => b.SubBranches.Count == 0 && b.Assignments.Count == 0);
        
        bool eligibleToTakeFrom(ArmyAssignment assgn, float ratio)
        {
            if (assgn.Armies.Count == 0) return false;
            var need = needs[assgn];
            if (need == 0f) return true;
            var assgnRatio = assgn.GetPowerPointsAssigned(key.Data) / need;
            if (assgnRatio > 1.5f * ratio) return true;
            return false;
        }
    }

    public IEnumerable<IDeploymentNode> GetDescendentNodes()
    {
        return Assignments
            .Concat<IDeploymentNode>(SubBranches)
            .Concat(SubBranches.SelectMany(s => s.GetDescendentNodes()));
    }
    public IEnumerable<ArmyAssignment> GetDescendentAssignments()
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