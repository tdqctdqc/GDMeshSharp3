
using System;

public class TurnMiddleState : TurnState
{
    public TurnMiddleState(LogicKey key, OrderHolder holder) 
        : base(key, holder)
    {
        _majorModules = new LogicModule[]
        {
        };
        _minorModules = new LogicModule[] 
        {
        };
    }

    public override void Calculate()
    {
        _orders.CalcAiOrdersParallel(_key);
        // _orders.CalcAiOrdersSequentially(_key);
    }

    public override bool ReadyForNext()
    {
        return _orders.CheckReadyForFrame(_key.Data);
    }
}