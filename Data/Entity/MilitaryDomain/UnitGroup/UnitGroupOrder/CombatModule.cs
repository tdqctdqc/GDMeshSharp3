
using System.Collections.Generic;
using System.Linq;

public class CombatModule : LogicModule
{
    public override void Calculate(List<RegimeTurnOrders> orders, LogicWriteKey key)
    {
        new CombatCalculator().Calculate(key);
    }
}