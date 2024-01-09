using System.Linq;

public static class UnitExt
{
    public static float GetPowerPoints(this Unit u, Data d)
    {
        return u.Troops.GetEnumerableModel(d)
            .Sum(kvp => kvp.Value * kvp.Key.GetPowerPoints());
    }
    public static float GetAttackPoints(this Unit u, Data d)
    {
        return u.Troops.GetEnumerableModel(d)
            .Sum(kvp => kvp.Value * kvp.Key.GetAttackPoints());
    }
    public static float GetHitPoints(this Unit u, Data d)
    {
        return u.Troops.GetEnumerableModel(d)
            .Sum(kvp => kvp.Value * kvp.Key.Hitpoints);
    }
    public static UnitGroup GetGroup(this Unit u, Data d)
    {
        return d.Military.UnitAux.UnitByGroup[u];
    }
}