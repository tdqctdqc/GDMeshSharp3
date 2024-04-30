using System.Collections.Generic;
using Godot;

public interface IMakeable
{
    MakeableAttribute Makeable { get; }
}
public class MakeableAttribute : IItemAttribute, ITroopAttribute
{
    public IdCount<IModel> BuildCosts { get; private set; }
    public IdCount<IModel> MaintainCosts { get; private set; }

    public MakeableAttribute(IdCount<IModel> buildCosts,
        IdCount<IModel> maintainCosts)
    {
        BuildCosts = buildCosts;
        MaintainCosts = maintainCosts;
    }
    
}