using System;
using System.Collections.Generic;
using System.Linq;

public class IncomeBudget
{
    public float BuyWishlistItemsRatio { get; private set; }
    public float BuyReserveItemsRatio { get; private set; }
    public void Calculate(Data data)
    {
        BuyWishlistItemsRatio = .5f;
        BuyReserveItemsRatio = .25f;
    }
}
