
using System.Collections.Generic;
using System.Linq;

public class CombatModule : LogicModule
{
    public override void Calculate(List<RegimeTurnOrders> orders, LogicWriteKey key)
    {
        var data = key.Data;
        var combatOrders = key.Data.GetAll<UnitGroup>()
            .Select(g => g.GroupOrder)
            .OfType<ICombatOrder>();
        var combatActions = combatOrders
            .AsParallel()
            .Select(o => o.DecideCombatAction(key.Data))
            .Where(a => a != null)
            .SelectMany(a => a)
            .ToDictionary(kvp => kvp.Key, kvp => kvp.Value);
        var combatResults = CombatCalculator.Calculate(combatActions, key.Data);
        
        key.SendMessage(combatResults);
    }
}