using System;
using System.Collections.Generic;
using System.Linq;

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
        var friendPower = data.Society.Regimes.Entities
            .Where(e => e == _regime || e.RelationWith(_regime, data).Alliance)
            .Sum(r => r.GetPowerScore(data));
        var enemyPower = data.Society.Regimes.Entities
            .Where(e => e != _regime && e.RelationWith(_regime, data).Enemies)
            .Sum(r => r.GetPowerScore(data));
        if (friendPower > enemyPower * DesiredFriendToEnemyPowerRatio)
        {
            FindEnemies(data, orders, friendPower, enemyPower);
        }

    }

    private void FindEnemies(Data data, MajorTurnOrders orders, float friendPower,
        float enemyPower)
    {
        var enemyPowerToFill = (friendPower - enemyPower) / DesiredFriendToEnemyPowerRatio;
        var neutralNeighbors = _regime.Polygons.Entities(data)
            .SelectMany(p => p.Neighbors.Entities(data))
            .Select(p => p.Regime.Entity(data))
            .Where(r =>
            {
                if (r == _regime) return false;
                var rel = r.RelationWith(_regime, data);
                return rel.Enemies == false && rel.Alliance == false;
            })
            .ToHashSet();
        if (neutralNeighbors.Count == 0) return;
        var newEnemy = neutralNeighbors.FirstOrDefault(n => n.GetPowerScore(data) <= enemyPowerToFill);
        if (newEnemy != null)
        {
            
        }
    }
}
