
using System;

public class TurnStartState : TurnState
{
    public TurnStartState(OrderHolder orders, Data data, Action<Message> queueMessage) 
        : base(data, queueMessage, orders)
    {
        _majorModules = new LogicModule[]
        {
            new DefaultLogicModule(() => new PrepareNewHistoriesProcedure()),
            new DefaultLogicModule(() => new SetContextProcedure()),
            new TrimFrontsModule(),
            new DefaultLogicModule(() => new FinishedTurnStartCalcProc())
        };
        _minorModules = new LogicModule[]
        {
            new DefaultLogicModule(() => new SetContextProcedure()),
            new DefaultLogicModule(() => new FinishedTurnStartCalcProc())
        };
    }
}