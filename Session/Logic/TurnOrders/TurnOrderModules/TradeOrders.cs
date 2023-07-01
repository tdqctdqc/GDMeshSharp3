using System;
using System.Collections.Generic;
using System.Linq;
using MessagePack;

public class TradeOrders : TurnOrderModule
{
    public List<SellOrder> SellOrders { get; private set; }
    public List<BuyOrder> BuyOrders { get; private set; }

    public static TradeOrders Construct()
    {
        return new TradeOrders(new List<SellOrder>(), new List<BuyOrder>());
    }
    [SerializationConstructor] private TradeOrders(List<SellOrder> sellOrders, List<BuyOrder> buyOrders)
    {
        SellOrders = sellOrders;
        BuyOrders = buyOrders;
    }

    public override void WriteToResult(Data data, LogicResults res)
    {
        
    }
}
