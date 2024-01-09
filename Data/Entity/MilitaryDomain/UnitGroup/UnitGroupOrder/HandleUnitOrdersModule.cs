
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

public class HandleUnitOrdersModule : LogicModule
{
    public override void Calculate(List<RegimeTurnOrders> orders, 
        LogicWriteKey key)
    {
        var data = key.Data;
        var proc = HandleUnitOrdersProcedure.Construct();
        Parallel.ForEach(data.GetAll<UnitGroup>(), 
            group =>
            {
                group.Order.Handle(group, key, proc);
            }
        );
        key.SendMessage(proc);

        var combatOrders = key.Data.GetAll<UnitGroup>()
            .Select(g => g.Order)
            .OfType<ICombatOrder>();
        var combatActions = combatOrders
            .AsParallel()
            .SelectMany(o => o.DecideCombatAction(key.Data))
            .ToDictionary(kvp => kvp.Key, kvp => kvp.Value);

        var combatResults = CombatCalculator.Calculate(combatActions, key.Data);
        
        key.SendMessage(combatResults);
    }
}