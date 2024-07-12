using System.Collections.Generic;
using Godot;

public interface IMakeable
{
    MakeableAttribute Makeable { get; }
}
public class MakeableAttribute : IItemAttribute, ITroopAttribute
{
    public IdCount<Item> BuildCosts { get; private set; }
    public IdCount<Item> MaintainCosts { get; private set; }

    public MakeableAttribute(IdCount<Item> buildCosts,
        IdCount<Item> maintainCosts)
    {
        BuildCosts = buildCosts;
        MaintainCosts = maintainCosts;
    }
    
}