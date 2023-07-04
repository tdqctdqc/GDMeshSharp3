using System;
using System.Collections.Generic;
using System.Linq;
using Godot;

public class TradeableItem : Item
{
    public float DefaultPrice { get; private set; }
    public TradeableItem(string name, Color color, float defaultPrice, params ItemAttribute[] attributes) 
        : base(name, color, attributes)
    {
        DefaultPrice = defaultPrice;
    }
}
