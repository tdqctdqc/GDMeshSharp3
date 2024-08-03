using System;
using System.Collections.Generic;
using System.Linq;
using Godot;

public class DiplomacyAi
{
    public static float DesiredFriendToRivalPowerRatio = 1.5f;
    public static float DesiredFriendToEnemyPowerRatio = 2f;
    public DiplomacyAi()
    {
    }

    public void CalculateMinor(Alliance alliance, LogicKey key, MinorTurnOrders orders)
    {
    }

    public void Calculate(Alliance alliance, RegimeTurnOrders orders, LogicKey key)
    {
        var alliancePower = alliance.GetPowerScore(key.Data);
        var rivalPower = alliance.GetRivals(key.Data)
            .Sum(a => a.GetPowerScore(key.Data));
        if (alliancePower > rivalPower * DesiredFriendToRivalPowerRatio)
        {
            ChooseRivals(alliance, key.Data, orders, alliancePower, rivalPower);
        }
        if (rivalPower / DesiredFriendToRivalPowerRatio > alliancePower)
        {
            ProposeInvitations(alliance, orders, alliancePower, rivalPower, key);
        }
        ProposeWars(alliance, key.Data, orders, alliancePower, rivalPower);
        DecideOnProposals(alliance, key);
    }

    private void DecideOnProposals(Alliance alliance, LogicKey key)
    {
        var proposals = alliance.PendingProposals(key.Data);
        foreach (var proposal in proposals)
        {
            var decision = proposal.GetDecisionForAi(key.Data);
            var decisionProc = new DecideOnProposalProcedure(decision, proposal.Id);
            key.SendMessage(decisionProc);
        }
    }
    private void ChooseRivals(Alliance alliance, Data data, RegimeTurnOrders orders, 
        float friendPower,
        float rivalPower)
    {
        var rivalPowerToFill = (friendPower - rivalPower) / DesiredFriendToRivalPowerRatio;
        var neutralNeighbors = alliance.GetNeighborAlliances(data)
            .Where(a =>
            {
                if (a == alliance) return false;
                var power = a.GetPowerScore(data);
                if (power > rivalPowerToFill) return false;
                return alliance.IsRivals(a, data) == false;
            })
            .ToHashSet();
        if (neutralNeighbors.Count == 0) return;
        var newRival = neutralNeighbors
            .OrderBy(e => GetRivalScore(alliance, e, data))
            .FirstOrDefault();
        if (newRival != null && Game.I.Random.Randf() < .5f)
        {
            var proc = new DeclareRivalProcedure(alliance.Id,
                newRival.Id);
            orders.Procedures.Add(proc);
        }
    }

    private float GetRivalScore(Alliance alliance, Alliance target,
        Data data)
    {
        var targetPolys = target.Members.Entities(data)
            .SelectMany(r => r.GetCells(data));
        var targetNeighborPolys = targetPolys
            .Where(p => p.GetNeighbors(data)
                .Any(np => np.Controller.Fulfilled()
                           && np.Controller.Get(data).GetAlliance(data) == alliance)).Count();
        var pCount = targetPolys.Count();
        if (pCount == 0) return 0f;
        return targetNeighborPolys / pCount;
    }
    private void ProposeInvitations( 
        Alliance alliance,
        RegimeTurnOrders orders, float friendPower,
        float rivalPower, LogicKey key)
    {
        var regime = orders.Regime.Get(key.Data);
        if (regime.IsMajor == false) return;
        
        var friendPowerToFill = rivalPower * DesiredFriendToRivalPowerRatio - friendPower;
        if (friendPowerToFill < 0f) return;
        var neutralNeighbors = alliance
            .GetNeighborAlliances(key.Data)
            .Where(a =>
            {
                if (a == alliance) return false;
                if (a.Leader.Get(key.Data).IsMajor) return false;
                return alliance.IsRivals(a, key.Data) == false;
            })
            .ToHashSet();
        if (neutralNeighbors.Count == 0) return;
        var newFriend = neutralNeighbors
            .OrderBy(e => GetFriendScore(alliance, e, key.Data))
            .FirstOrDefault();
        if (newFriend != null && Game.I.Random.Randf() < .5f)
        {
            var proposal = AllianceMergeProposal.Construct(alliance, newFriend, key.Data);
            var proc = MakeProposalProcedure.Construct(proposal, key);
            orders.Procedures.Add(proc);
        }
    }
    private float GetFriendScore(Alliance alliance, Alliance target, Data data)
    {
        var res = 0f;
        var power = target.GetPowerScore(data);
        res += power * 2f;
        var ourEnemies = alliance.GetRivals(data);
        var sharedEnemies = target.GetRivals(data)
            .Where(ourEnemies.Contains);
        if (sharedEnemies.Any())
        {
            res += sharedEnemies.Sum(e => e.GetPowerScore(data));
        }
        return res;
    }

    private void ProposeWars(
        Alliance alliance,
        Data data, RegimeTurnOrders orders, float friendPower,
        float rivalPower)
    {
        if (friendPower < rivalPower * DesiredFriendToRivalPowerRatio) return;
        var enemyPower = alliance.GetAtWar(data).Sum(a => a.GetPowerScore(data));
        if (enemyPower * DesiredFriendToEnemyPowerRatio > friendPower) return;
        var nonEnemyRivals = alliance.GetRivals(data)
            .Except(alliance.GetAtWar(data)).ToList();
        if (nonEnemyRivals.Count == 0) return;
        if (Game.I.Random.Randf() < .1f)
        {
            var target = nonEnemyRivals.OrderBy(r => r.GetPowerScore(data)).First();
            var proc = new DeclareWarProcedure(target.Id, alliance.Id);
            orders.Procedures.Add(proc);
        }
    }
}
