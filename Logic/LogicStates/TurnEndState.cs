
using System;

public class TurnEndState : TurnState
{
    public TurnEndState(LogicWriteKey key, OrderHolder orders) 
        : base(key, orders)
    {
        _majorModules = new LogicModule[]
        {
            new DoTurnOrderProceduresModule(),
            new ProduceConstructModule(),
            new CleanUpFinishedConstructionsModule(),
            new FoodAndPopGrowthModule(),
            new FinanceModule(),
            new TradeModule(),
            new AllianceOrdersModule(),
            new DefaultLogicModule(() => new TickProcedure()),
            new ClearOrdersModule(orders)
        };
        _minorModules = new LogicModule[]
        {
            new DoTurnOrderProceduresModule(),
            new HandleUnitOrdersModule(),
            new CombatModule(),
            new DefaultLogicModule(() => new TickProcedure()),
            new ClearOrdersModule(orders)
        };
    }
}