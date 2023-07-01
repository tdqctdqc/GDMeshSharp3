using System;
using System.Collections.Generic;
using System.Linq;

public class IncomeBudget
{
    public float BuyItemsRatio { get; private set; }
    public void Calculate(Data data)
    {
        BuyItemsRatio = .5f;
    }
}
