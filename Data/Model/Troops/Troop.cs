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
    public int Echelon { get; private set; }
    public Icon Icon { get; private set; }
    public MakeableAttribute Makeable { get; private set; }
    public void Make(Regime r, float amount, ProcedureWriteKey key)
    {
        r.Stock.Stock.Add(this, amount);
    }

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
        Domain = domain;
        Makeable = makeable;
        Icon = Icon.Create(name.ToLower(), Vector2I.One);
    }
}