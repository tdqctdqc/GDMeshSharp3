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

    public void CalculateMajor(Regime regime, LogicWriteKey key, MajorTurnOrders orders)
    {
        AssignFreeUnitsToGroups(regime, key, orders);
    }
    public void CalculateMinor(Regime regime, LogicWriteKey key, MinorTurnOrders orders)
    {
        FillExposedFronts(regime, key.Data, orders);
        foreach (var forceAssignment in ForceAssignments)
        {
            forceAssignment.CalculateOrders(orders, key);
        }
    }

    private void AssignFreeUnitsToGroups(Regime regime, LogicWriteKey key, MajorTurnOrders orders)
    {
        var freeUnits = key.Data.Military.UnitAux.UnitByRegime[regime]
            ?.Where(u => u != null)
            .Where(u => key.Data.Military.UnitAux.UnitByGroup[u] == null);
        if (freeUnits == null || freeUnits.Count() == 0) return;
        var numGroups = Mathf.CeilToInt((float)freeUnits.Count() / PreferredGroupSize);
        var newGroups = Enumerable.Range(0, numGroups)
            .Select(i => new List<int>())
            .ToList();
        
        var iter = 0;
        foreach (var freeUnit in freeUnits)
        {
            var group = iter % numGroups;
            Game.I.Logger.Log($"adding unit to group pre", LogType.Temp);

            newGroups.ElementAt(group).Add(freeUnit.Id);
            iter++;
        }
        foreach (var newGroup in newGroups)
        {
            Game.I.Logger.Log($"creating new group from {newGroup.Count()} units", LogType.Temp);

            UnitGroup.Create(orders.Regime.Entity(key.Data),
                newGroup, key);
        }
    }

    private void FillExposedFronts(Regime regime, Data data, MinorTurnOrders orders)
    {
        var fronts = data.Military.FrontAux.Fronts[regime];
        if (fronts == null) return;
        var exposed = fronts
            ?.Where(f => ForceAssignments.Any(a => a is FrontAssignment fa && fa.Front == f) == false)
            .ToList();
        if (exposed.Count == 0) return;
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