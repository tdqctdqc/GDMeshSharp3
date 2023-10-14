
using Godot;
using MessagePack;

public class Unit : Entity
{
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
        var u = new Unit(-1, regime.MakeRef(), template.MakeRef(),
            IdCount<Troop>.Construct(template.TroopCounts),
            position, template.Domain);
        key.Create(u);
        return u;
    }

    [SerializationConstructor] protected Unit(int id, 
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
}