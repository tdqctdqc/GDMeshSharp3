using Godot;
using System.Collections.Generic;
using System.Linq;

public class Troop : Item, IMakeable, IIconed
{
    public string DisplayName { get; private set; }
    public float HardAttack { get; private set; }
    public float SoftAttack { get; private set; }
    public float Hitpoints { get; private set; }
    public float Hardness { get; private set; }
    public float Accuracy { get; private set; }
    public float Evasion { get; private set; }
    public int Echelon { get; private set; }
    public int Range { get; private set; }
    public float[] TargetChance { get; private set; }
    public float FrontLength { get; private set; }
    public float BreakthroughMult { get; private set; }
    public MakeableAttribute Makeable { get; private set; }
    public TroopDomain Domain { get; private set; }
    public HashSet<Technology> Prereqs { get; private set; }
    
    public Troop()
    {
    }
}