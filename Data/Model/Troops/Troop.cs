using Godot;
using System.Collections.Generic;

public class Troop : IModel, IMakeable
{
    public string Name { get; private set; }
    public int Id { get; private set; }
    public int Level { get; private set; }
    public float HardAttack { get; private set; }
    public float SoftAttack { get; private set; }
    public float Hitpoints { get; private set; }
    public float Hardness { get; private set; }
    public int Echelon { get; private set; }
    public Icon Icon { get; private set; }
    public MakeableAttribute Makeable { get; private set; }
    public TroopDomain Domain { get; private set; }
    public Troop(string name, int level, float hardAttack, 
        float softAttack, 
        float hitpoints, 
        float hardness,
        int echelon,
        TroopDomain domain,
        MakeableAttribute makeable)
    {
        Name = name;
        Level = level;
        HardAttack = hardAttack;
        SoftAttack = softAttack;
        Hitpoints = hitpoints;
        Hardness = hardness;
        Echelon = echelon;
        Domain = domain;
        Makeable = makeable;
        Icon = Icon.Create(Name.ToLower(), Icon.AspectRatio._1x1, 50f);
    }
}