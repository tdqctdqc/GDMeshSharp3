using System;
using System.Collections.Generic;
using System.Linq;
using Godot;

public abstract class CompoundDeploymentBranch : DeploymentBranch
{
    public HashSet<DeploymentBranch> Assignments { get; }
    public override IEnumerable<IDeploymentNode> Children() => 
        Reserve.Yield<IDeploymentNode>().Union(Assignments);
    protected CompoundDeploymentBranch(HashSet<DeploymentBranch> assignments,
        ERef<Regime> regime, int id) : base(regime, id)
    {
        Assignments = assignments;
    }

    public override void AdjustWithin(LogicWriteKey key)
    {
        // if (Assignments.Count < 2) return;
        //
        // var data = key.Data;
        //
        // var max = maxSatisfied();
        // var min = minSatisfied();
        // var iter = 0;
        // while (iter < Assignments.Count * 2 
        //        && max.ratio > min.ratio * 1.5f)
        // {
        //     var g = max.fa.GetPossibleTransferGroup(key);
        //     if (g != null)
        //     {
        //         max.fa.Groups.Transfer(g, min.fa, key);
        //     }
        //     max = maxSatisfied();
        //     min = minSatisfied();
        //     iter++;
        // }
        //
        // (float ratio, DeploymentBranch fa) maxSatisfied()
        // {
        //     var max = Assignments.MaxBy(fa => fa.GetSatisfiedRatio(data));
        //     return (max.GetSatisfiedRatio(data), max);
        // }
        //
        // (float ratio, DeploymentBranch fa) minSatisfied()
        // {
        //     var min = Assignments.MinBy(fa => fa.GetSatisfiedRatio(data));
        //     return (min.GetSatisfiedRatio(data), min);
        // }
    }
    public override float GetPowerPointNeed(Data d)
    {
        return Assignments.Sum(fa => fa.GetPowerPointNeed(d));
    }
    public override UnitGroup GetPossibleTransferGroup(LogicWriteKey key)
    {
        foreach (var n in Assignments.OrderByDescending(s => s.GetSatisfiedRatio(key.Data)))
        {
            var g = n.GetPossibleTransferGroup(key);
            if (g != null) return g;
        }
        return null;
    }
}

public static class ICompoundForceAssignmentExt
{
    public static void ShiftGroups<T>(this T assgn, 
        LogicWriteKey key)
        where T : CompoundDeploymentBranch
    {
        
    }
}

