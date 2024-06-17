
using System;

public class TurnStartState : TurnState
{
    public TurnStartState(LogicWriteKey key, OrderHolder orders) 
        : base(key, orders)
    {
        _majorModules = new LogicModule[]
        {
            new MigrationModule(),
            new ProductionModule(),
            new DefaultLogicModule(() => new FinishedTurnStartCalcProc())
        };
        _minorModules = new LogicModule[]
        {
            new DefaultLogicModule(() => new FinishedTurnStartCalcProc())
        };
    }
}