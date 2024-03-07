using System;
using System.Collections.Generic;
using System.Linq;
using Godot;

public class DiplomacyAi
{
    private Alliance _alliance;
    public static float DesiredFriendToRivalPowerRatio = 1.5f;
    public static float DesiredFriendToEnemyPowerRatio = 2f;
    public DiplomacyAi(Alliance alliance)
    {
        _alliance = alliance;
    }

    public void CalculateMinor(LogicWriteKey key, MinorTurnOrders orders)
    {
        DecideOnProposals(key, orders);
    }

    public void Calculate(RegimeTurnOrders orders, LogicWriteKey key)
    {
        var alliancePower = _alliance.GetPowerScore(key.Data);
        var rivalPower = _alliance.GetRivals(key.Data)
            .Sum(a => a.GetPowerScore(key.Data));
        if (alliancePower > rivalPower * DesiredFriendToRivalPowerRatio)
        {
            ChooseRivals(key.Data, orders, alliancePower, rivalPower);
        }
        if (rivalPower / DesiredFriendToRivalPowerRatio > alliancePower)
        {
            ProposeInvitations(orders, alliancePower, rivalPower, key);
        }
        ProposeWars(key.Data, orders, alliancePower, rivalPower);
    }

    private void DecideOnProposals(LogicWriteKey key, MinorTurnOrders orders)
    {
        var proposals = _alliance.PendingProposals(key.Data);
        foreach (var proposal in proposals)
        {
            var decision = proposal.GetDecisionForAi(key.Data);
            var decisionProc = new DecideOnProposalProcedure(decision, proposal.Id);
            key.SendMessage(decisionProc);
        }
    }
    private void ChooseRivals(Data data, RegimeTurnOrders orders, float friendPower,
        float rivalPower)
    {
        var rivalPowerToFill = (friendPower - rivalPower) / DesiredFriendToRivalPowerRatio;
        var neutralNeighbors = _alliance.GetNeighborAlliances(data)
            .Where(a =>
            {
                if (a == _alliance) return false;
                var power = a.GetPowerScore(data);
                if (power > rivalPowerToFill) return false;
                return _alliance.IsRivals(a, data) == false;
            })
            .ToHashSet();
        if (neutralNeighbors.Count == 0) return;
        var newRival = neutralNeighbors
            .OrderBy(e => GetRivalScore(_alliance, e, data))
            .FirstOrDefault();
        if (newRival != null && Game.I.Random.Randf() < .5f)
        {
            var proc = new DeclareRivalProcedure(_alliance.Id,
                newRival.Id);
            orders.Procedures.Add(proc);
        }
    }

    private float GetRivalScore(Alliance alliance, Alliance target,
        Data data)
    {
        var targetPolys = target.Members.Items(data).SelectMany(r => r.GetPolys(data));
        var targetNeighborPolys = targetPolys
            .Where(p => p.Neighbors.Items(data)
                .Any(np => np.OwnerRegime.Fulfilled()
                           && np.OwnerRegime.Entity(data).GetAlliance(data) == alliance)).Count();
        var pCount = targetPolys.Count();
        if (pCount == 0) return 0f;
        return targetNeighborPolys / pCount;
    }
    private void ProposeInvitations( 
        RegimeTurnOrders orders, float friendPower,
        float rivalPower, LogicWriteKey key)
    {
        var regime = orders.Regime.Entity(key.Data);
        if (regime.IsMajor == false) return;
        
        var friendPowerToFill = rivalPower * DesiredFriendToRivalPowerRatio - friendPower;
        if (friendPowerToFill < 0f) return;
        var neutralNeighbors = _alliance
            .GetNeighborAlliances(key.Data)
            .Where(a =>
            {
                if (a == _alliance) return false;
                if (a.Leader.Entity(key.Data).IsMajor) return false;
                return _alliance.IsRivals(a, key.Data) == false;
            })
            .ToHashSet();
        if (neutralNeighbors.Count == 0) return;
        var newFriend = neutralNeighbors
            .OrderBy(e => GetFriendScore(_alliance, e, key.Data))
            .FirstOrDefault();
        if (newFriend != null && Game.I.Random.Randf() < .5f)
        {
            var proposal = AllianceMergeProposal.Construct(_alliance, newFriend, key.Data);
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
        if (sharedEnemies.Count() > 0)
        {
            res += sharedEnemies.Sum(e => e.GetPowerScore(data));
        }
        return res;
    }

    private void ProposeWars(Data data, RegimeTurnOrders orders, float friendPower,
        float rivalPower)
    {
        if (friendPower < rivalPower * DesiredFriendToRivalPowerRatio) return;
        var enemyPower = _alliance.GetAtWar(data).Sum(a => a.GetPowerScore(data));
        if (enemyPower * DesiredFriendToEnemyPowerRatio > friendPower) return;
        var nonEnemyRivals = _alliance.GetRivals(data)
            .Except(_alliance.GetAtWar(data)).ToList();
        if (nonEnemyRivals.Count == 0) return;
        if (Game.I.Random.Randf() < .1f)
        {
            var target = nonEnemyRivals.OrderBy(r => r.GetPowerScore(data)).First();
            var proc = new DeclareWarProcedure(target.Id, _alliance.Id);
            orders.Procedures.Add(proc);
        }
    }
}
