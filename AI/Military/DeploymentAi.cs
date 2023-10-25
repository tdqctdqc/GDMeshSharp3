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
        var fronts = regime.Military.Fronts.Items(data);
        var openFronts = fronts
            ?.Where(f => ForceAssignments
                .Any(a => a is FrontAssignment fa && fa.Front == f) == false);
        var freeGroups = data.Military.UnitAux.UnitGroupByRegime[regime]
            ?.Where(g => ForceAssignments.Any(f => f.Groups.Contains(g)) == false)
            .ToHashSet();
        
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
}