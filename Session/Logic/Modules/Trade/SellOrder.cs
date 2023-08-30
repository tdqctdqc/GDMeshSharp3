using System;
using System.Collections.Generic;
using System.Linq;

public class SellOrder
{
    public int RegimeId { get; set; }
    public int ItemId { get; set; }
    public int Quantity { get; set; }
    public SellOrder(int itemId, int regimeId, int quantity)
    {
        ItemId = itemId;
        RegimeId = regimeId;
        Quantity = quantity;
    }
}
