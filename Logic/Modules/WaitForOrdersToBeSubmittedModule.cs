
using System.Collections.Generic;

public class WaitForOrdersToBeSubmittedModule : LogicModule
{
    private OrderHolder _holder;
    public WaitForOrdersToBeSubmittedModule(OrderHolder holder)
    {
        _holder = holder;
    }
    public override LogicResults Calculate(List<RegimeTurnOrders> orders, Data data)
    {
        var res = new LogicResults();
        _holder.CalcAiTurnOrders(data);
        while (_holder.CheckReadyForFrame(data, data.BaseDomain.GameClock.MajorTurn(data)) == false)
        {
            
        }

        return res;
    }
}