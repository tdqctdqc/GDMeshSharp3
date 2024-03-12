using System.Linq;
using Godot;

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

    public static bool Hostile(this Unit u, Alliance a, Data d)
    {
        return u.Regime.Get(d).GetAlliance(d).IsRivals(a, d);
    }

    public static Troop GetMaxPowerTroop(this Unit unit, Data data)
    {
        var maxPowerId = unit.Troops.Contents
            .MaxBy(kvp =>
            {
                var unit = data.Models.GetModel<Troop>(kvp.Key);
                var power = kvp.Value * unit.GetPowerPoints();
                return power;
            }).Key;
        return data.Models.GetModel<Troop>(maxPowerId);
    }

    public static Vector2 GetHealth(this Unit unit, Data data)
    {
        var totalPp = unit.Troops.GetEnumerableModel(data)
            .Sum(kvp => kvp.Key.GetPowerPoints() * kvp.Value);
        var templatePp = unit.Template.Get(data).TroopCounts.GetEnumerableModel(data)
            .Sum(kvp => kvp.Key.GetPowerPoints() * kvp.Value);
        return new Vector2(totalPp, templatePp);
    }
}