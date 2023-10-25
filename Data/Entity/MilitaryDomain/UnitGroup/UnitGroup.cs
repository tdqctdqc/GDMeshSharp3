
using System.Collections.Generic;
using Godot;
using MessagePack;

public class UnitGroup : Entity
{
    public EntityRef<Regime> Regime { get; private set; }
    public EntRefCol<Unit> Units { get; private set; }
    public UnitOrder Order { get; private set; }
    public static UnitGroup Create(Regime r, IEnumerable<int> unitIds, ICreateWriteKey key)
    {
        var id = key.Data.IdDispenser.TakeId();
        var units = EntRefCol<Unit>.Construct(nameof(Units), id, unitIds.ToHashSet(), key.Data);
        var u = new UnitGroup(id, r.MakeRef(), units,
            new DoNothingUnitOrder());
        GD.Print("made unit group");
        key.Create(u);
        return u;
    }
    [SerializationConstructor] private UnitGroup(int id,
        EntityRef<Regime> regime, 
        EntRefCol<Unit> units,
        UnitOrder order) 
        : base(id)
    {
        Regime = regime;
        Units = units;
        Order = order;
    }

    public static void ChangeUnitGroup(Unit u, 
        UnitGroup oldG, UnitGroup newG,
        ProcedureWriteKey key)
    {
        oldG?.Units.Remove(u, key);
        newG?.Units.Add(u, key);
        key.Data.Military.UnitAux.UnitChangedGroup.Invoke(u, newG, oldG);
    }
}