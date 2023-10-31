
using System;
using System.Collections.Generic;

public class ClearOrdersModule : LogicModule
{
    private OrderHolder _orders;

    public ClearOrdersModule(OrderHolder orders)
    {
        _orders = orders;
    }

    public override void Calculate(List<RegimeTurnOrders> orders, LogicWriteKey key)
    {
        _orders.Clear();
    }
}