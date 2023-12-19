using System;
using System.Collections.Generic;
using System.Linq;
using Godot;

public interface ICompoundForceAssignment
{
    HashSet<ForceAssignment> Assignments { get; }
}

public static class ICompoundForceAssignmentExt
{
    public static void ShiftGroups<T>(this T assgn, LogicWriteKey key)
        where T : ForceAssignment, ICompoundForceAssignment
    {
        if (assgn.Assignments.Count < 2) return;
        var data = key.Data;

        var max = maxSatisfied();
        var min = minSatisfied();
        var iter = 0;
        while (iter < assgn.Assignments.Count * 2 
               && max.ratio > min.ratio * 1.5f)
        {
            var g = max.fa.RequestGroup(key);
            if (g != null)
            {
                min.fa.GroupIds.Add(g.Id);
            }
            max = maxSatisfied();
            min = minSatisfied();
            iter++;
        }

        (float ratio, ForceAssignment fa) maxSatisfied()
        {
            var max = assgn.Assignments.MaxBy(fa => fa.GetSatisfiedRatio(data));
            return (max.GetSatisfiedRatio(data), max);
        }

        (float ratio, ForceAssignment fa) minSatisfied()
        {
            var min = assgn.Assignments.MinBy(fa => fa.GetSatisfiedRatio(data));
            return (min.GetSatisfiedRatio(data), min);
        }
    }
    public static void AssignFreeGroups<T>(this T assgn, LogicWriteKey key)
        where T : ForceAssignment, ICompoundForceAssignment
    {
        var occupiedGroups = assgn.Assignments
            .SelectMany(fa => fa.GroupIds)
            .Select(g => key.Data.Get<UnitGroup>(g));
        var freeGroups = assgn.Groups(key.Data)
            ?.Except(occupiedGroups)
            ?.ToList();
        if (freeGroups == null || freeGroups.Count == 0) return;
        
        Assigner.Assign<ForceAssignment, UnitGroup>(
            assgn.Assignments,
            fa => fa.GetPowerPointNeed(key.Data),
            fsa => fsa.Groups(key.Data),
            g => g.GetPowerPoints(key.Data),
            freeGroups.ToHashSet(),
            (fa, g) => fa.GroupIds.Add(g.Id),
            (fa, g) => g.GetPowerPoints(key.Data));
    }

    public static void TakeAwayGroupCompound<T>(this T assgn, UnitGroup toTake, LogicWriteKey key)
        where T : ForceAssignment, ICompoundForceAssignment
    {
        var subAssgns = assgn.Assignments.Where(fa => fa.GroupIds.Contains(toTake.Id));
        if (subAssgns.Count() > 1) throw new Exception();
        if (subAssgns.Count() == 1)
        {
            subAssgns.First().TakeAwayGroup(toTake, key);
        }

        assgn.GroupIds.Remove(toTake.Id);
    }
}

