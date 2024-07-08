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
    public static float GetAttackPoints(this Unit u, Data d)
    {
        return u.Troops.GetEnumModel(d)
            .Sum(kvp => kvp.Value * kvp.Key.GetAttackPoints());
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
    public static Troop GetMaxPowerTroop(this UnitTemplate unit, Data data)
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
        var totalPp = unit.Troops.GetEnumModel(data)
            .Sum(kvp => kvp.Key.GetPowerPoints() * kvp.Value);
        var templatePp = unit.Template.Get(data).Troops.GetEnumModel(data)
            .Sum(kvp => kvp.Key.GetPowerPoints() * kvp.Value);
        return new Vector2(totalPp, templatePp);
    }
    
    public static Control GetUnitDisplay(this Unit u, Data d)
    {
        return u.GetMaxPowerTroop(d).Icon
            .GetLabeledIcon<HBoxContainer>($"{u.Template.Get(d).Name}: {u.GetPowerPoints(d).Round(2)} / {u.Template.Get(d).GetPowerPoints(d).Round(2)}",
                10f);
    }
}