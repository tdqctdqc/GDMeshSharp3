using System;
using System.Collections.Generic;
using System.Linq;

public class BuyOrder
{
    public int ItemId { get; private set; }
    public int RegimeId { get; private set; }
    public int Quantity { get; private set; }
    public BuyOrder(int itemId, int regimeId, int quantity)
    {
        ItemId = itemId;
        Quantity = quantity;
        RegimeId = regimeId;
    }
}
