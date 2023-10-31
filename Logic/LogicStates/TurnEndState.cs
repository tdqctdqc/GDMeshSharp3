
using System;

public class TurnEndState : TurnState
{
    public TurnEndState(LogicWriteKey key, OrderHolder orders) 
        : base(key, orders)
    {
        _majorModules = new LogicModule[]
        {
            new ProduceConstructModule(),
            new ConstructBuildingsModule(),
            new FoodAndPopGrowthModule(),
            new FinanceModule(),
            new TradeModule(),
            new ProposalsModule(),
            new FormUnitsAndGroupsModule(),
            new AllianceOrdersModule(),
            new DefaultLogicModule(() => new TickProcedure()),
            new ClearOrdersModule(orders)
        };
        _minorModules = new LogicModule[]
        {
            new HandleUnitOrdersModule(),
            new DefaultLogicModule(() => new TickProcedure()),
            new ClearOrdersModule(orders)
        };
    }
}