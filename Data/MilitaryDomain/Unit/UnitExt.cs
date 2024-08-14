using System.Linq;
using Godot;
using MathNet.Numerics;
using Control = Godot.Control;

public static class UnitExt
{
    public static float GetPowerPoints(this Unit u, Data d)
    {
        if (u.Troops.Contents.Count == 0) return 0f; 
        return u.Troops.GetEnumModel(d)
            .Sum(kvp =>
            {
                var v = kvp.Value * kvp.Key.GetPowerPoints();
                if (float.IsNaN(v)) return 0f;
                return v;
            });
    }
    public static float GetPowerPointsWeightedMorale(this Unit u, Data d)
    {
        if (u.Troops.Contents.Count == 0) return 0f; 
        var raw = u.Troops.GetEnumModel(d)
            .Sum(kvp =>
            {
                var v = kvp.Value * kvp.Key.GetPowerPoints();
                if (float.IsNaN(v)) return 0f;
                return v;
            });
        return raw * Mathf.Sqrt(u.Morale);
    }
    public static float GetHitPoints(this Unit u, Data d)
    {
        return u.Troops.GetEnumModel(d)
            .Sum(kvp => kvp.Value * kvp.Key.Hitpoints);
    }
    public static Army GetArmy(this Unit u, Data d)
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
    public static Troop GetMaxPowerTroop(this IdCount<Troop> troops, Data data)
    {
        var maxPowerId = troops.Contents
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
        var totalFrontLength = unit.Troops.GetEnumModel(data)
            .Sum(kvp => kvp.Key.TroopType.FrontLength * kvp.Value);
        var templateFrontLength = unit.Template.Get(data).Troops.GetEnumModel(data)
            .Sum(kvp => kvp.Key.FrontLength * kvp.Value);
        return new Vector2(totalFrontLength, templateFrontLength);
    }
    
    
    
}