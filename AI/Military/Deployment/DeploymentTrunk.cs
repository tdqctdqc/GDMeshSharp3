using System;
using System.Collections.Generic;
using System.Linq;
using Godot;

public abstract class DeploymentTrunk : DeploymentBranch
{
    public HashSet<DeploymentBranch> Branches { get; }
    public override IEnumerable<IDeploymentNode> Children()
        => Reserve.Yield<IDeploymentNode>().Union(Branches);
    protected DeploymentTrunk(HashSet<DeploymentBranch> branches,
        ERef<Regime> regime, int id, int parentId, ReserveAssignment reserve) 
        : base(regime, id, parentId, reserve)
    {
        Branches = branches;
    }
    public void ShiftGroups(DeploymentAi ai, LogicWriteKey key)
    {
        var data = key.Data;
        if (Branches.Count == 0) return;
        var max = maxSatisfied();
        var min = minSatisfied();
        var iter = 0;
        var maxIter = Branches.Count * 2 + Reserve.Groups.Count();
        while (iter < maxIter
               && max.ratio > min.ratio * 1.5f)
        {
            max.fa.PullGroup(ai, Reserve, key);
            if (Reserve.Groups.Count() > 0)
            {
                min.fa.PushGroup(ai, Reserve, key);
            }
            
            max = maxSatisfied();
            min = minSatisfied();
            iter++;
        }
        // if (iter == maxIter) throw new Exception();

        iter = 0;
        while (iter < maxIter
               && Reserve.Groups.Count() > 0
               && Reserve.ReserveThreshold > min.ratio)
        {
            min.fa.PushGroup(ai, Reserve, key);
            min = minSatisfied();
            iter++;
        }
        // if (iter == maxIter) throw new Exception();

        
        foreach (var b in Branches.OfType<DeploymentTrunk>())
        {
            b.ShiftGroups(ai, key);
        }
        
        (float ratio, DeploymentBranch fa) maxSatisfied()
        {
            var max = Branches.MaxBy(fa => fa.GetSatisfiedRatio(data));
            return (max.GetSatisfiedRatio(data), max);
        }
        
        (float ratio, DeploymentBranch fa) minSatisfied()
        {
            var min = Branches.MinBy(fa => fa.GetSatisfiedRatio(data));
            return (min.GetSatisfiedRatio(data), min);
        }
    }
    
    public override float GetPowerPointNeed(Data d)
    {
        return Branches.Sum(fa => fa.GetPowerPointNeed(d));
    }
    
    
    public override bool PullGroup(DeploymentAi ai, GroupAssignment transferTo,
        LogicWriteKey key)
    {
        if (Reserve.PullGroup(ai, transferTo, key)) return true;
        foreach (var n in Branches.OrderByDescending(s => s.GetSatisfiedRatio(key.Data)))
        {
            var received = n.PullGroup(ai, transferTo, key);
            if (received) return true;
        }
        return false;
    }
    public override bool PushGroup(DeploymentAi ai, GroupAssignment transferFrom,
        LogicWriteKey key)
    {
        foreach (var n in Branches
                     .OrderByDescending(s => s.GetSatisfiedRatio(key.Data)))
        {
            var received = n.PushGroup(ai, transferFrom, key);
            if (received) return true;
        }
        return false;
    }
}

public static class ICompoundForceAssignmentExt
{
    // public static void ShiftGroups<T>(this T assgn, 
    //     LogicWriteKey key)
    //     where T : DeploymentTrunk
    // {
    //     
    // }
}

