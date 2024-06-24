
using Godot;

public partial class TroopRes : Resource
{
    [Export] public string DisplayName { get; set; }
    [Export] public float HardAttack { get; set; }
    [Export] public float SoftAttack { get; set; }
    [Export] public float Hitpoints { get; set; }
    [Export] public float Hardness { get; set; }
    [Export] public int Echelon { get; set; }
    [Export] public float Accuracy { get; set; }
    [Export] public float Evasion { get; set; }
    [Export] public float Recon { get; set; }
    [Export] public float MilitaryCapCost { get; set; }
    [Export] public float FrontLength { get; set; }
    
}