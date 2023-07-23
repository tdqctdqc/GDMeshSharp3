using System;
using System.Collections.Generic;
using System.Linq;
using Godot;

public class DiplomacyAi
{
    private Regime _regime;
    public static float DesiredFriendToRivalPowerRatio = 1.5f;
    public static float DesiredFriendToEnemyPowerRatio = 2f;
    public DiplomacyAi(Regime regime)
    {
        _regime = regime;
    }
    
    public void Calculate(Data data, MajorTurnOrders orders)
    {
        var alliance = _regime.GetAlliance(data);
        var alliancePower = alliance.GetPowerScore(data);
        var rivalPower = alliance.Rivals.Items(data)
            .Sum(a => a.GetPowerScore(data));
        if (alliancePower > rivalPower * DesiredFriendToRivalPowerRatio)
        {
            FindEnemies(data, orders, alliancePower, rivalPower);
        }
        if (rivalPower / DesiredFriendToRivalPowerRatio > alliancePower)
        {
            FindFriends(data, orders, alliancePower, rivalPower);
        }
        ProposeWars(data, orders, alliancePower, rivalPower);
        DecideOnProposals(data, orders);
    }

    private void DecideOnProposals(Data data, MajorTurnOrders orders)
    {
        var proposals = _regime.GetAlliance(data).Proposals.Items(data)
            .Select(h => h.Value);
        foreach (var proposal in proposals)
        {
            if (proposal.Against.Contains(_regime.Id)
                || proposal.InFavor.Contains(_regime.Id)) continue;
            var decision = proposal.GetDecisionForAi(_regime, data);
            orders.DiplomacyOrders.ProposalDecisions[proposal.Id] = decision;
        }
    }
    private void FindEnemies(Data data, MajorTurnOrders orders, float friendPower,
        float rivalPower)
    {
        var rivalPowerToFill = (friendPower - rivalPower) / DesiredFriendToRivalPowerRatio;
        var regimeAlliance = _regime.GetAlliance(data);
        var neutralNeighbors = _regime.GetNeighborAlliances(data)
            .Where(a =>
            {
                if (a == regimeAlliance) return false;
                var power = a.GetPowerScore(data);
                if (power > rivalPowerToFill) return false;
                return regimeAlliance.Rivals.Contains(a) == false;
            })
            .ToHashSet();
        if (neutralNeighbors.Count == 0) return;
        var newRival = neutralNeighbors
            .OrderBy(e => GetRivalScore(regimeAlliance, e, data))
            .FirstOrDefault();
        if (newRival != null && Game.I.Random.Randf() < .5f)
        {
            var proposal = DeclareRivalProposal.Construct(_regime, newRival, data);
            proposal.InFavor.Add(_regime.Id);
            orders.DiplomacyOrders.ProposalsMade.Add(proposal);
        }
    }

    private float GetRivalScore(Alliance alliance, Alliance target, Data data)
    {
        var targetPolys = target.Members.Items(data).SelectMany(r => r.Polygons.Items(data));
        var targetNeighborPolys = targetPolys
            .Where(p => p.Neighbors.Items(data)
                .Any(np => np.Regime.Fulfilled()
                           && np.Regime.Entity(data).GetAlliance(data) == alliance)).Count();
        var pCount = targetPolys.Count();
        if (pCount == 0) return 0f;
        return targetNeighborPolys / pCount;
    }
    private void FindFriends(Data data, MajorTurnOrders orders, float friendPower,
        float rivalPower)
    {
        var friendPowerToFill = rivalPower * DesiredFriendToRivalPowerRatio - friendPower;
        if (friendPowerToFill < 0f) return;
        var regimeAlliance = _regime.GetAlliance(data);
        var neutralNeighbors = _regime.GetNeighborAlliances(data)
            .Where(a =>
            {
                if (a == regimeAlliance) return false;
                return regimeAlliance.Rivals.Contains(a) == false;
            })
            .ToHashSet();
        if (neutralNeighbors.Count == 0) return;
        var newFriend = neutralNeighbors
            .OrderBy(e => GetFriendScore(regimeAlliance, e, data))
            .FirstOrDefault();
        if (newFriend != null && Game.I.Random.Randf() < .5f)
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
        var ourEnemies = alliance.Rivals.Items(data);
        var sharedEnemies = target.Rivals.Items(data)
            .Where(ourEnemies.Contains);
        if (sharedEnemies.Count() > 0)
        {
            res += sharedEnemies.Sum(e => e.GetPowerScore(data));
        }
        return res;
    }

    private void ProposeWars(Data data, MajorTurnOrders orders, float friendPower,
        float rivalPower)
    {
        if (friendPower < rivalPower * DesiredFriendToRivalPowerRatio) return;
        var regimeAlliance = _regime.GetAlliance(data);
        var enemyPower = regimeAlliance.AtWar.Items(data).Sum(a => a.GetPowerScore(data));
        if (enemyPower * DesiredFriendToEnemyPowerRatio > friendPower) return;
        var nonEnemyRivals = regimeAlliance.Rivals.Items(data)
            .Except(regimeAlliance.AtWar.Items(data)).ToList();
        if (nonEnemyRivals.Count == 0) return;
        if (Game.I.Random.Randf() < .1f)
        {
            var target = nonEnemyRivals.OrderBy(r => r.GetPowerScore(data)).First();
            var proposal = DeclareWarProposal.Construct(_regime, target, data);
            orders.DiplomacyOrders.ProposalsMade.Add(proposal);
        }
    }
}
