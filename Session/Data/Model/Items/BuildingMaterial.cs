
using System.Collections.Generic;
using Godot;

public class BuildingMaterial : TradeableItem
{
    public BuildingMaterial() 
        : base("BuildingMaterial", Colors.OrangeRed, 1, 
            new []
            {
                new MakeableAttribute(
                    new Dictionary<Item, int>(),
                    1f
                    )
            })
    {
        
    }
}