
using System.Collections.Generic;
using Godot;

public class BuildingMaterial : TradeableItem, IMakeable
{
    public MakeableAttribute Makeable { get; private set; }
    public BuildingMaterial() 
        : base("BuildingMaterial", Colors.OrangeRed, 1)
    {
        Makeable = new MakeableAttribute(
            IdCount<Item>.Construct(new Dictionary<Item, float>()),
            1f
        );
    }
}