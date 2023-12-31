
using System;
using Godot;
using MessagePack;

public class Unit : Entity
{
    public static float MovePoints { get; private set; }
        = 100f;
    public EntityRef<Regime> Regime { get; private set; }
    public EntityRef<UnitTemplate> Template { get; private set; }
    public IdCount<Troop> Troops { get; private set; }
    public TroopDomain Domain { get; private set; }
    public MapPos Position { get; private set; }

    public static Unit Create(UnitTemplate template, 
        Regime regime,
        MapPos pos,
        ICreateWriteKey key)
    {
        var u = new Unit(key.Data.IdDispenser.TakeId(), regime.MakeRef(), template.MakeRef(),
            IdCount<Troop>.Construct(template.TroopCounts),
            pos, template.Domain);
        key.Create(u);
        return u;
    }

    [SerializationConstructor] private Unit(int id, 
        EntityRef<Regime> regime,
        EntityRef<UnitTemplate> template,
        IdCount<Troop> troops, MapPos position, TroopDomain domain) 
        : base(id)
    {
        Regime = regime;
        Template = template;
        Troops = troops;
        Position = position;
        Domain = domain;
    }

    public void SetPosition(MapPos pos, ProcedureWriteKey key)
    {
        if (float.IsNaN(pos.Pos.X) || float.IsNaN(pos.Pos.Y))
        {
            throw new Exception("bad unit pos");
        }

        Position = pos;
    }
}