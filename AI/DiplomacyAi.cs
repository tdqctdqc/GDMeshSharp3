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
        
        if (enemyPower / DesiredFriendToEnemyPowerRatio > alliancePower)
        {
            FindFriends(data, orders, alliancePower, enemyPower);
        }
        DecideOnProposals(data, orders);
    }

    private void DecideOnProposals(Data data, MajorTurnOrders orders)
    {
        var proposals = _regime.GetAlliance(data).Proposals;
        foreach (var kvp in proposals)
        {
            var proposal = kvp.Value;
            if (proposal.Against.Contains(_regime.Id)
                || proposal.InFavor.Contains(_regime.Id)) continue;
            var decision = proposal.GetDecisionForAi(_regime, data);
            orders.DiplomacyOrders.ProposalDecisions[proposal.Id] = decision;
        }
    }
    private void FindEnemies(Data data, MajorTurnOrders orders, float friendPower,
        float enemyPower)
    {
        var enemyPowerToFill = (friendPower - enemyPower) / DesiredFriendToEnemyPowerRatio;
        var regimeAlliance = _regime.GetAlliance(data);
        var neutralNeighbors = _regime.Polygons.Entities(data)
            .SelectMany(p => p.Neighbors.Entities(data).Where(e => e.Regime.Fulfilled()))
            .Select(p => p.Regime.Entity(data).GetAlliance(data))
            .Where(a =>
            {
                if (a == regimeAlliance) return false;
                var power = a.GetPowerScore(data);
                if (power > enemyPowerToFill) return false;
                return regimeAlliance.Enemies.Contains(a) == false;
            })
            .ToHashSet();
        if (neutralNeighbors.Count == 0) return;
        var newEnemy = neutralNeighbors
            .OrderBy(e => GetEnemyScore(regimeAlliance, e, data))
            .FirstOrDefault();
        if (newEnemy != null)
        {
            var proposal = DeclareEnemyProposal.Construct(_regime, newEnemy, data);
            proposal.InFavor.Add(_regime.Id);
            orders.DiplomacyOrders.ProposalsMade.Add(proposal);
        }
    }

    private float GetEnemyScore(Alliance alliance, Alliance target, Data data)
    {
        var targetPolys = target.Members.Entities(data).SelectMany(r => r.Polygons.Entities(data));
        var targetNeighborPolys = targetPolys
            .Where(p => p.Neighbors.Entities(data)
                .Any(np => np.Regime.Fulfilled()
                           && np.Regime.Entity(data).GetAlliance(data) == alliance)).Count();
        var pCount = targetPolys.Count();
        if (pCount == 0) return 0f;
        return targetNeighborPolys / pCount;
    }
    private void FindFriends(Data data, MajorTurnOrders orders, float friendPower,
        float enemyPower)
    {
        var friendPowerToFill = enemyPower * DesiredFriendToEnemyPowerRatio - friendPower;
        if (friendPowerToFill < 0f) return;
        var regimeAlliance = _regime.GetAlliance(data);
        var neutralNeighbors = _regime.Polygons.Entities(data)
            .SelectMany(p => p.Neighbors.Entities(data).Where(e => e.Regime.Fulfilled()))
            .Select(p => p.Regime.Entity(data).GetAlliance(data))
            .Where(a =>
            {
                if (a == regimeAlliance) return false;
                return regimeAlliance.Enemies.Contains(a) == false;
            })
            .ToHashSet();
        if (neutralNeighbors.Count == 0) return;
        var newFriend = neutralNeighbors
            .OrderBy(e => GetFriendScore(regimeAlliance, e, data))
            .FirstOrDefault();
        if (newFriend != null)
        {
            var proposal = AllianceMergeProposal.Construct(_regime, newFriend, data);
            proposal.InFavor.Add(_regime.Id);
            orders.DiplomacyOrders.ProposalsMade.Add(proposal);
        }
    }
    private float GetFriendScore(Alliance alliance, Alliance target, Data data)
    {
        var res = 0f;
        var power = target.GetPowerScore(data);
        res += power * 2f;
        var ourEnemies = alliance.Enemies.Entities(data);
        var sharedEnemies = target.Enemies.Entities(data)
            .Where(ourEnemies.Contains);
        if (sharedEnemies.Count() > 0)
        {
            res += sharedEnemies.Sum(e => e.GetPowerScore(data));
        }
        return res;
    }
}
