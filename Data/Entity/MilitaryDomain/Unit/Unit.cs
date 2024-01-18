
using System;
using Godot;
using MessagePack;

public class Unit : Entity
{
    public EntityRef<Regime> Regime { get; private set; }
    public EntityRef<UnitTemplate> Template { get; private set; }
    public IdCount<Troop> Troops { get; private set; }
    public MapPos Position { get; private set; }
    public float Radius() => 5f;
    public static Unit Create(UnitTemplate template, 
        Regime regime,
        MapPos pos,
        ICreateWriteKey key)
    {
        var u = new Unit(key.Data.IdDispenser.TakeId(), regime.MakeRef(), template.MakeRef(),
            IdCount<Troop>.Construct(template.TroopCounts),
            pos);
        key.Create(u);
        return u;
    }

    [SerializationConstructor] private Unit(int id, 
        EntityRef<Regime> regime,
        EntityRef<UnitTemplate> template,
        IdCount<Troop> troops, MapPos position) 
        : base(id)
    {
        Regime = regime;
        Template = template;
        Troops = troops;
        Position = position;
    }

    public void SetPosition(MapPos pos, ProcedureWriteKey key)
    {
        Position = pos;
    }
}