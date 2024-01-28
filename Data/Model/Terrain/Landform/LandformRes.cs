using Godot;

public partial class LandformRes : Resource
{
    [Export] public string Name { get; set; }
    [Export] public float MinRoughness { get; set; }
    [Export] public float FertilityMod { get; set; }
    [Export] public float DarkenFactor { get; set; }
    [Export] public Color Color { get; set; }
    [Export] public bool IsWater { get; set; }
}