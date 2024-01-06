using System.Linq;

public static class UnitExt
{
    public static float GetPowerPoints(this Unit u, Data d)
    {
        return u.Troops.GetEnumerableModel(d)
            .Sum(kvp => kvp.Value * kvp.Key.GetPowerPoints());
    }

    public static UnitGroup GetGroup(this Unit u, Data d)
    {
        return d.Military.UnitAux.UnitByGroup[u];
    }
}