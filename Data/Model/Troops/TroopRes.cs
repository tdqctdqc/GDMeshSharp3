
using Godot;

public partial class TroopRes : Resource
{
    [Export] public string DisplayName { get; set; }
    [Export] public float HardAttack { get; set; }
    [Export] public float SoftAttack { get; set; }
    [Export] public float Hitpoints { get; set; }
    [Export] public float Hardness { get; set; }
    [Export] public int Echelon { get; set; }
}