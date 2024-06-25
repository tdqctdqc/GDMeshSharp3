
using System.Linq;
using MessagePack;

public class UnitCombatInfo
{
    public ERef<Unit> Unit { get; private set; }
    public IdCount<Troop> Active { get; private set; }
    public IdCount<Troop> Kills { get; private set; }
    public float ActiveFrontSize { get; private set; }
    public UnitCombatInfo(Unit u, Data d)
    {
        Unit = u.MakeRef();
        Active = IdCount<Troop>.Construct(u.Troops);
        Kills = IdCount<Troop>.Construct();
        ActiveFrontSize = Active.GetEnumerableModel(d)
            .Sum(v => v.Key.FrontLength * v.Value);
    }

    [SerializationConstructor] private UnitCombatInfo(
        ERef<Unit> unit, IdCount<Troop> active, 
        IdCount<Troop> kills,
        float activeFrontSize)
    {
        Unit = unit;
        Active = active;
        Kills = kills;
        ActiveFrontSize = activeFrontSize;
    }

    public float ProportionLosses(Data d)
    {
        var power = Unit.Get(d).GetPowerPoints(d);
        var actual = Active.GetEnumerableModel(d).Sum(
            v => v.Key.GetPowerPoints() * v.Value);
        return 1f - actual / power;
    }

    public void AddKill(Troop troop, float amt)
    {
        Kills.Add(troop, amt);
    }

    public void AddLoss(Troop troop, float amt)
    {
        Active.Remove(troop, amt);
        ActiveFrontSize -= troop.FrontLength * amt;
    }
}