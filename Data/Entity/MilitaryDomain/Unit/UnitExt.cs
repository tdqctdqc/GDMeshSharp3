using System.Linq;

public static class UnitExt
{
    public static float GetPowerPoints(this Unit u, Data d)
    {
        return u.Troops.GetEnumerableModel(d)
            .Sum(kvp => kvp.Value * kvp.Key.GetPowerPoints());
    }
}