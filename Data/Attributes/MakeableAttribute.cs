using System.Collections.Generic;
using Godot;

public interface IMakeable
{
    MakeableAttribute Makeable { get; }
}
public class MakeableAttribute : IItemAttribute, ITroopAttribute
{
    public IdCount<Item> ItemCosts { get; private set; }
    public float IndustrialCost { get; private set; }

    public MakeableAttribute(IdCount<Item> itemCosts, float industrialCost)
    {
        ItemCosts = itemCosts;
        IndustrialCost = industrialCost;
    }
}