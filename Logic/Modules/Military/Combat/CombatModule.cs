
using System;
using System.Collections.Generic;
using System.Linq;

public class CombatModule : LogicModule
{
    public override void Calculate(List<RegimeTurnOrders> orders, LogicKey key)
    {
        new CombatCalculator().Calculate(key);
    }
}