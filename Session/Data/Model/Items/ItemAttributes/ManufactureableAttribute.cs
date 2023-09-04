using System.Collections.Generic;
using Godot; 

public class ManufactureableAttribute : ItemAttribute
{
    public Dictionary<Item, int> ItemCosts { get; private set; }
    public float IndustrialCost { get; private set; }

    public ManufactureableAttribute(Dictionary<Item, int> itemCosts, float industrialCost)
    {
        ItemCosts = itemCosts;
        IndustrialCost = industrialCost;
    }
}