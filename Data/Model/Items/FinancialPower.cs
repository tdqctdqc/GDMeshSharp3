using System;
using System.Collections.Generic;
using System.Linq;
using Godot;

public class FinancialPower : TradeableItem
{
    public FinancialPower()
        : base(nameof(FinancialPower), Colors.Green, 
            2f)
    {
    }
}
