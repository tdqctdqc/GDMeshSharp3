using System;
using System.Collections.Generic;
using System.Linq;
using Godot;

public class DiplomacyAi
{
    private Regime _regime;
    public static float DesiredFriendToEnemyPowerRatio = 1.5f;
    public DiplomacyAi(Regime regime)
    {
        _regime = regime;
    }

    public void Calculate(Data data, MajorTurnOrders orders)
    {
        var alliance = _regime.GetAlliance(data);

        var alliancePower = alliance.GetPowerScore(data);
        var enemyPower = alliance.Enemies.Entities(data)
            .Sum(a => a.GetPowerScore(data));
        if (alliancePower > enemyPower * DesiredFriendToEnemyPowerRatio)
        {
            FindEnemies(data, orders, alliancePower, enemyPower);
        }
    }

    private void FindEnemies(Data data, MajorTurnOrders orders, float friendPower,
        float enemyPower)
    {
        var enemyPowerToFill = (friendPower - enemyPower) / DesiredFriendToEnemyPowerRatio;
        var neutralNeighbors = _regime.Polygons.Entities(data)
            .SelectMany(p => p.Neighbors.Entities(data).Where(e => e.Regime.Fulfilled()))
            .Select(p => p.Regime.Entity(data).GetAlliance(data))
            .Where(a =>
            {
                if (a == _regime.GetAlliance(data)) return false;
                return _regime.GetAlliance(data).Enemies.Contains(a) == false;
            })
            .ToHashSet();
        if (neutralNeighbors.Count == 0) return;
        var newEnemy = neutralNeighbors.FirstOrDefault(n => n.GetPowerScore(data) <= enemyPowerToFill);
        if (newEnemy != null)
        {
            var proposal = DeclareEnemyProposal.Construct(_regime, newEnemy, data);
            proposal.InFavor.Add(_regime.Id);
            orders.DiplomacyOrders.AllianceProposals.Add(proposal);
        }
    }
}
