using System;
using System.Collections.Generic;
using System.Linq;

public class SellOrder
{
    public int RegimeId { get; private set; }
    public int ItemId { get; private set; }
    public int Quantity { get; private set; }
    public SellOrder(int itemId, int regimeId, int minPrice, int quantity)
    {
        ItemId = itemId;
        RegimeId = regimeId;
        Quantity = quantity;
    }
}
