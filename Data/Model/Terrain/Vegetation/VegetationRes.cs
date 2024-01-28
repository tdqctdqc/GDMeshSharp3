using Godot;

public partial class VegetationRes : Resource
{
    [Export] public string Name { get; set; }
    [Export] public Color Color { get; set; }
    [Export] public float MinMoisture { get; set; }
    [Export] public float FertilityMod { get; set; }
    [Export] public float MovementCostMult { get; set; }
}