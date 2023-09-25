using Godot;
using System.Collections.Generic;

public class Troop : IModel
{
    public string Name { get; private set; }
    IReadOnlyList<IModelAttribute> IModel.AttributeList => Attributes;
    public AttributeHolder<ITroopAttribute> Attributes { get; }
    public int Id { get; private set; }
    public int Level { get; private set; }
    public TroopType TroopType { get; private set; }
    public float HardAttack { get; private set; }
    public float SoftAttack { get; private set; }
    public float Hitpoints { get; private set; }
    public float IndustrialCost { get; private set; }
    public Dictionary<Item, float> BuildCosts { get; private set; }
    public Icon Icon { get; private set; }
    // public TroopAttribute[] Attributes { get; private set; }

    public Troop(string name, int level, float hardAttack, 
        float softAttack, 
        float hitpoints, float industrialCost, Dictionary<Item, float> buildCosts, 
        TroopType troopType,
        ITroopAttribute[] attributes)
    {
        Name = name;
        Level = level;
        HardAttack = hardAttack;
        SoftAttack = softAttack;
        Hitpoints = hitpoints;
        IndustrialCost = industrialCost;
        BuildCosts = buildCosts;
        Attributes = new AttributeHolder<ITroopAttribute>(attributes);
        TroopType = troopType;
        Icon = Icon.Create(Name.ToLower(), Icon.AspectRatio._1x1, 50f);
    }
}