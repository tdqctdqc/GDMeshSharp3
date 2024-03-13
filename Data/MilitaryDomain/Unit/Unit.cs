
using System;
using Godot;
using MessagePack;

public class Unit : Entity, ICombatGraphNode
{
    public ERef<Regime> Regime { get; private set; }
    public ERef<UnitTemplate> Template { get; private set; }
    public IdCount<Troop> Troops { get; private set; }
    public MapPos Position { get; private set; }
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
        ERef<Regime> regime,
        ERef<UnitTemplate> template,
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
        var old = Position;
        Position = pos;
        key.Data.Notices.Military.UnitChangedPos.Invoke(
            this, pos, old);
    }

    public override void CleanUp(StrongWriteKey key)
    {
        var g = this.GetGroup(key.Data);
        if (g != null)
        {
            g.Units.Remove(this, key);
            if (g.Units.Count() == 0)
            {
                key.Data.RemoveEntity(g.Id, key);
            }
        }
    }
    
}