
using System;
using System.Collections.Generic;

public class WaitForOrdersToBeSubmittedModule : LogicModule
{
    private OrderHolder _holder;
    public WaitForOrdersToBeSubmittedModule(OrderHolder holder)
    {
        _holder = holder;
    }
    public override void Calculate(List<RegimeTurnOrders> orders, Data data, Action<Message> sendMessage)
    {
        var res = new LogicResults();
        _holder.CalcAiOrders(data);
        while (_holder.CheckReadyForFrame(data, data.BaseDomain.GameClock.MajorTurn(data)) == false)
        {
            
        }
    }
}