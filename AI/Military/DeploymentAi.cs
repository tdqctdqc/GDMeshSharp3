using System.Collections.Generic;
using System.Linq;
using Godot;

public class DeploymentAi
{
    public HashSet<ForceAssignment> ForceAssignments { get; private set; }
    public DeploymentAi()
    {
        ForceAssignments = new HashSet<ForceAssignment>();
    }
    public void Calculate(Regime regime, LogicWriteKey key, MinorTurnOrders orders)
    {
        CreateFronts(regime, key);
        FillFronts(regime, key.Data);
        foreach (var forceAssignment in ForceAssignments)
        {
            forceAssignment.CalculateOrders(orders, key);
        }
    }
    public IEnumerable<FrontAssignment> GetFrontAssignments()
    {
        return ForceAssignments.SelectWhereOfType<FrontAssignment>();
    }
    
    private void CreateFronts(Regime regime, LogicWriteKey key)
    {
        var alliance = regime.GetAlliance(key.Data);
        var allianceAi = key.Data.HostLogicData.AllianceAis[alliance];
        var responsibility = allianceAi.MilitaryAi.AreasOfResponsibility[regime];

        ForceAssignments.RemoveWhere(fa => fa is FrontAssignment);
        
        var unions = UnionFind.Find(responsibility, (wp1, wp2) =>
        {
            return responsibility.Contains(wp1) == responsibility.Contains(wp2);
        }, wp => wp.GetNeighboringWaypoints(key.Data));
        
        foreach (var union in unions)
        {
            var contactLines = RegimeMilitaryAi.GetContactLines(regime, union, key.Data);
            foreach (var contactLine in contactLines)
            {
                var front = Front.Construct(regime, contactLine.Select(wp => wp.Id),
                    key);
                var assgn = new FrontAssignment(front);
                ForceAssignments.Add(assgn);
            }
        }
    }
    private void FillFronts(Regime regime, Data data)
    {
        var frontAssgns = GetFrontAssignments();
        var occupiedGroups = ForceAssignments.SelectMany(fa => fa.Groups);
        var freeGroups = data.Military.UnitAux.UnitGroupByRegime[regime]
            ?.Except(occupiedGroups)
            ?.ToList();
        if (freeGroups == null || freeGroups.Count == 0) return;

        Assigner<FrontAssignment, UnitGroup>.Assign(
            frontAssgns, 
            fa => -fa.GetPowerPointRatio(data),
            freeGroups.ToHashSet(),
            (fa, g) => fa.Groups.Add(g),
            (fa, g) => g.GetPowerPoints(data));
    }
}