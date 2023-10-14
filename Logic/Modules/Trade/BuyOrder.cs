using System;
using System.Collections.Generic;
using System.Linq;

public class BuyOrder
{
    public int ItemId { get; set; }
    public int RegimeId { get; set; }
    public int Quantity { get; set; }
    public BuyOrder(int itemId, int regimeId, int quantity)
    {
        ItemId = itemId;
        Quantity = quantity;
        RegimeId = regimeId;
    }
}
