using System;
using System.Collections.Generic;
using System.Linq;
using Godot;

public class FinancialPower : Item
{
    public FinancialPower() 
        : base(nameof(FinancialPower), Colors.Green, 
            new ItemAttribute[]{})
    {
    }
}
