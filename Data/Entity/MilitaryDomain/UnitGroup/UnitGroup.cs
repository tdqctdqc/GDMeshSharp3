
using System.Collections.Generic;
using MessagePack;

public class UnitGroup : Entity
{
    public EntityRef<Regime> Regime { get; private set; }
    public EntRefCol<Unit> Units { get; private set; }
    public UnitOrder Order { get; private set; }
    public static UnitGroup Construct(Regime r, ICreateWriteKey key)
    {
        var id = key.Data.IdDispenser.TakeId();
        var units = EntRefCol<Unit>.Construct(nameof(Units), id, new HashSet<int>(), key.Data);
        return new UnitGroup(id, r.MakeRef(), units,
            new DoNothingUnitOrder());
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
}