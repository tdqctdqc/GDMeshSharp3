using System.Collections.Generic;
using System.Linq;
using Godot;

public class DeploymentAi
{
    public HashSet<ForceAssignment> ForceAssignments { get; private set; }
    private static int PreferredGroupSize = 7;
    public DeploymentAi()
    {
        ForceAssignments = new HashSet<ForceAssignment>();
    }

    public void CalculateMajor(Regime regime, Data data, MajorTurnOrders orders)
    {
        AssignFreeUnitsToGroups(regime, data, orders);
    }
    public void CalculateMinor(Regime regime, Data data, MinorTurnOrders orders)
    {
        FillExposedFronts(regime, data, orders);
    }

    private void AssignFreeUnitsToGroups(Regime regime, Data data, MajorTurnOrders orders)
    {
        var freeUnits = data.Military.UnitAux.UnitByRegime[regime]
            ?.Where(u => u != null)
            .Where(u => data.Military.UnitAux.UnitByGroup[u] == null);
        if (freeUnits == null) return;
        var numGroups = Mathf.CeilToInt((float)freeUnits.Count() / PreferredGroupSize);
        var newGroups = Enumerable.Range(0, numGroups)
            .Select(i => new List<int>());
        
        var iter = 0;
        foreach (var freeUnit in freeUnits)
        {
            var group = iter % numGroups;
            newGroups.ElementAt(group).Add(freeUnit.Id);
            iter++;
        }
        orders.Military.NewGroupUnits.AddRange(newGroups);
    }

    private void FillExposedFronts(Regime regime, Data data, MinorTurnOrders orders)
    {
        var fronts = data.Military.FrontAux.Fronts[regime];
        if (fronts == null) return;
        var exposed = fronts
            ?.Where(f => ForceAssignments.Any(a => a is FrontAssignment fa && fa.Front == f) == false)
            .ToList();
        if (exposed.Count == 0) return;
        GD.Print($"{exposed.Count} exposed fronts");
        var occupiedGroups = ForceAssignments.SelectMany(f => f.Groups).ToHashSet();
        
        
        var freeGroups = data.Military.UnitAux.UnitGroupByRegime[regime]
            ?.Except(occupiedGroups)
            ?.ToList();
        if (freeGroups == null || freeGroups.Count == 0) return;
        var frontGroups = exposed
            .ToDictionary(f => f, f => new List<UnitGroup>());
        var iter = 0;
        for (var i = 0; i < freeGroups.Count; i++)
        {
            var group = freeGroups[i];
            var front = exposed.Modulo(iter);
            iter++;
            frontGroups[front].Add(group);
        }
        
        foreach (var kvp in frontGroups)
        {
            var assignment = new FrontAssignment(kvp.Key);
            foreach (var unitGroup in kvp.Value)
            {
                assignment.Groups.Add(unitGroup);
            }

            ForceAssignments.Add(assignment);
        }
    }
}