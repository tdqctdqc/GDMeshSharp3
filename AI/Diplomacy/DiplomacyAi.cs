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

    public void CalculateMinor(Regime regime, LogicKey key, MinorTurnOrders orders)
    {
    }

    public void Calculate(Regime regime, RegimeTurnOrders orders, LogicKey key)
    {
        var regimePower = regime.GetPowerScore(key.Data);
        var rivalPower = regime.GetRivals(key.Data)
            .Sum(a => a.GetPowerScore(key.Data));
        if (regimePower > rivalPower * DesiredFriendToRivalPowerRatio)
        {
            ChooseRivals(regime, key.Data, orders, regimePower, rivalPower);
        }
        ProposeWars(regime, key.Data, orders, regimePower, rivalPower);
    }

    
    private void ChooseRivals(Regime regime, Data data, RegimeTurnOrders orders, 
        float friendPower,
        float rivalPower)
    {
        var rivalPowerToFill = (friendPower - rivalPower) / DesiredFriendToRivalPowerRatio;
        var neutralNeighbors = regime.GetNeighborRegimes(data)
            .Where(a =>
            {
                if (a == regime) return false;
                var power = a.GetPowerScore(data);
                if (power > rivalPowerToFill) return false;
                return regime.IsRivals(a, data) == false;
            })
            .ToHashSet();
        if (neutralNeighbors.Count == 0) return;
        var newRival = neutralNeighbors
            .OrderBy(e => GetRivalScore(regime, e, data))
            .FirstOrDefault();
        if (newRival != null && Game.I.Random.Randf() < .5f)
        {
            var proc = new DeclareRivalProcedure(regime.MakeRef(),
                newRival.MakeRef());
            orders.Procedures.Add(proc);
        }
    }

    private float GetRivalScore(Regime regime, Regime target,
        Data data)
    {
        var targetPolys = target.GetCells(data);
        var targetNeighborPolys = targetPolys
            .Where(p => p.GetNeighbors(data)
                .Any(np => np.Controller.Fulfilled()
                           && np.Controller.Get(data) == regime)).Count();
        var pCount = targetPolys.Count();
        if (pCount == 0) return 0f;
        return targetNeighborPolys / pCount;
    }
    
    private float GetFriendScore(Regime alliance, Regime target, Data data)
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
        Regime regime,
        Data data, RegimeTurnOrders orders, float friendPower,
        float rivalPower)
    {
        if (friendPower < rivalPower * DesiredFriendToRivalPowerRatio) return;
        var enemyPower = regime.GetAtWar(data).Sum(a => a.GetPowerScore(data));
        if (enemyPower * DesiredFriendToEnemyPowerRatio > friendPower) return;
        var nonEnemyRivals = regime.GetRivals(data)
            .Except(regime.GetAtWar(data)).ToList();
        if (nonEnemyRivals.Count == 0) return;
        if (Game.I.Random.Randf() < .1f)
        {
            var target = nonEnemyRivals.OrderBy(r => r.GetPowerScore(data)).First();
            var proc = new DeclareWarProcedure(target.MakeRef(), regime.MakeRef());
            orders.Procedures.Add(proc);
        }
    }
}
