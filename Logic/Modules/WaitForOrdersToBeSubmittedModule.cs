
using System;
using System.Collections.Generic;

public class WaitForOrdersToBeSubmittedModule : LogicModule
{
    private OrderHolder _holder;
    public WaitForOrdersToBeSubmittedModule(OrderHolder holder)
    {
        _holder = holder;
    }
    public override void Calculate(List<RegimeTurnOrders> orders, LogicWriteKey key)
    {
        var res = new LogicResults();
        _holder.CalcAiOrders(key);
        while (_holder.CheckReadyForFrame(key.Data, key.Data.BaseDomain.GameClock.MajorTurn(key.Data)) == false)
        {
            
        }
    }
}