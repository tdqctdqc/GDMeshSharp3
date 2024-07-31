
using System;

public class TurnMiddleState : TurnState
{
    public TurnMiddleState(LogicKey key, OrderHolder holder) 
        : base(key, holder)
    {
        _majorModules = new LogicModule[]
        {
            // new WaitForOrdersToBeSubmittedModule(holder)
        };
        _minorModules = new LogicModule[] 
        {
            // new WaitForOrdersToBeSubmittedModule(holder)
        };
    }

    public override void Calculate()
    {
        _orders.CalcAiOrdersAsync(_key);
    }

    public override bool ReadyForNext()
    {
        return _orders.CheckReadyForFrame(_key.Data);
    }
}