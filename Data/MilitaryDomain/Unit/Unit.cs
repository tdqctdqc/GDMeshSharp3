
using System;
using Godot;
using MessagePack;

public class Unit : Entity
{
    public ERef<Regime> Regime { get; private set; }
    public ERef<UnitTemplate> Template { get; private set; }
    public IdCount<Troop> Troops { get; private set; }
    public float Morale { get; private set; }
    public static Unit Create(UnitTemplate template, 
        IdCount<Troop> troops, 
        Regime regime,
        IHostWriteKey key)
    {
        var u = new Unit(key.Data.IdDispenser.TakeId(), 
            regime.MakeRef(), template.MakeRef(),
            IdCount<Troop>.Construct(troops),
            1f);
        key.Create(u);
        return u;
    }

    [SerializationConstructor] private Unit(int id, 
        ERef<Regime> regime,
        ERef<UnitTemplate> template,
        IdCount<Troop> troops,
        float morale) 
        : base(id)
    {
        Regime = regime;
        Template = template;
        Troops = troops;
        Morale = morale;
    }

    public override void CleanUp(StrongWriteKey key)
    {
        var g = this.GetArmy(key.Data);
        if (g != null)
        {
            g.Units.Remove(this, key);
        }
    }
}