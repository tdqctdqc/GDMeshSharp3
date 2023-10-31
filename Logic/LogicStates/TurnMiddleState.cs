
using System;

public class TurnMiddleState : TurnState
{
    public TurnMiddleState(LogicWriteKey key, OrderHolder holder) 
        : base(key, holder)
    {
        _majorModules = new LogicModule[]
        {
            new WaitForOrdersToBeSubmittedModule(holder)
        };
        _minorModules = new LogicModule[] 
        {
            new WaitForOrdersToBeSubmittedModule(holder)
        };
    }
}