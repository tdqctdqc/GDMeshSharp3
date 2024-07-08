using Godot;
using System.Collections.Generic;

public class Troop : IModel, IMakeable, IIconed
{
    public string Name { get; private set; }
    public string DisplayName { get; private set; }
    public int Id { get; private set; }
    public float HardAttack { get; private set; }
    public float SoftAttack { get; private set; }
    public float Hitpoints { get; private set; }
    public float Hardness { get; private set; }
    public float Accuracy { get; private set; }
    public float Evasion { get; private set; }
    public float Recon { get; private set; }
    public int Echelon { get; private set; }
    public int Range { get; private set; }
    public float[] TargetChances { get; private set; }
    public float FrontLength { get; private set; }
    public float BreakthroughMult { get; private set; }
    public Icon Icon { get; private set; }
    public MakeableAttribute Makeable { get; private set; }

    public TroopDomain Domain { get; private set; }
    public Troop(string name, 
        TroopDomain domain,
        MakeableAttribute makeable)
    {
        var res = GD.Load<TroopRes>($"res://Data/Model/Troops/{name}.tres");
        Name = name;
        DisplayName = res.DisplayName;
        HardAttack = res.HardAttack;
        SoftAttack = res.SoftAttack;
        Hitpoints = res.Hitpoints;
        Hardness = res.Hardness;
        Echelon = res.Echelon;
        Accuracy = res.Accuracy;
        Evasion = res.Evasion;
        Recon = res.Recon;
        FrontLength = res.FrontLength;
        Range = res.Range;
        TargetChances = res.TargetChances;
        Domain = domain;
        Makeable = makeable;
        Icon = Icon.Create(name.ToLower(), Vector2I.One);
    }
}