using System.Collections.Generic;
using Godot; 

public class MakeableAttribute : IItemAttribute, IModelAttribute
{
    public Dictionary<Item, int> ItemCosts { get; private set; }
    public float IndustrialCost { get; private set; }

    public MakeableAttribute(Dictionary<Item, int> itemCosts, float industrialCost)
    {
        ItemCosts = itemCosts;
        IndustrialCost = industrialCost;
    }
}