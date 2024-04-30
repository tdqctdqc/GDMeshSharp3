
using System;

public class TurnStartState : TurnState
{
    public TurnStartState(LogicWriteKey key, OrderHolder orders) 
        : base(key, orders)
    {
        _majorModules = new LogicModule[]
        {
            new DefaultLogicModule(() => new SetContextProcedure()),
            new ProductionModule(),
            new FoodAndPopGrowthModule(),
            new DefaultLogicModule(() => new FinishedTurnStartCalcProc())
        };
        _minorModules = new LogicModule[]
        {
            new DefaultLogicModule(() => new SetContextProcedure()),
            new DefaultLogicModule(() => new FinishedTurnStartCalcProc())
        };
    }
}