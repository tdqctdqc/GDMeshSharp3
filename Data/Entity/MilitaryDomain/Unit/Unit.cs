
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
    public Vector2 Position { get; private set; }

    public static Unit Create(UnitTemplate template, 
        Regime regime,
        Vector2 position,
        ICreateWriteKey key)
    {
        var u = new Unit(key.Data.IdDispenser.TakeId(), regime.MakeRef(), template.MakeRef(),
            IdCount<Troop>.Construct(template.TroopCounts),
            position, template.Domain);
        key.Create(u);
        return u;
    }

    [SerializationConstructor] private Unit(int id, 
        EntityRef<Regime> regime,
        EntityRef<UnitTemplate> template,
        IdCount<Troop> troops, Vector2 position, TroopDomain domain) 
        : base(id)
    {
        Regime = regime;
        Template = template;
        Troops = troops;
        Position = position;
        Domain = domain;
    }

    public void SetPosition(Vector2 pos, ProcedureWriteKey key)
    {
        if (float.IsNaN(pos.X) || float.IsNaN(pos.Y))
        {
            throw new Exception("bad unit pos");
        }
        Position = pos.ClampPosition(key.Data);
    }
}