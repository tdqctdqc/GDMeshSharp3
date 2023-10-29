
using System;

public class TurnMiddleState : TurnState
{
    public TurnMiddleState(OrderHolder holder, Data data, 
        Action<Message> sendMessage) : base(data, sendMessage, holder)
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