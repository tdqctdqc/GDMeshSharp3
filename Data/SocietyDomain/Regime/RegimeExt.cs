
using System.Collections.Generic;
using System.Linq;
using Godot;

public static class RegimeExt
{
    public static Color GetUnitColor(this Regime r)
    {
        return r.PrimaryColor
            .Interpolate(Colors.Black, .25f);
    }
    public static Color GetMapColor(this Regime r)
    {
        return r.PrimaryColor
            .Interpolate(Colors.Gray, .25f);
    }
    public static IEnumerable<Unit> GetUnits(this Regime r, Data d)
    {
        return d.Military.UnitAux.UnitByRegime[r];
    }
    public static IEnumerable<UnitTemplate> GetUnitTemplates(this Regime r, Data d)
    {
        return d.Military.UnitAux.UnitTemplates[r];
    }
    public static bool IsPlayerRegime(this Regime r, Data data)
    {
        return data.BaseDomain.PlayerAux.ByRegime.ContainsKey(r);
    }
    public static bool IsLocalPlayerRegime(this Regime r, Data data)
    {
        return data.BaseDomain.PlayerAux.LocalPlayer.Regime.Get(data) == r;
    }
    public static Player GetPlayer(this Regime r, Data data)
    {
        return data.BaseDomain.PlayerAux.ByRegime[r];
    }

    public static IEnumerable<MapPolygon> GetPolys(this Regime r, Data data)
    {
        return data.Planet.PolygonAux.PolysByRegime[r];
    }
    public static IEnumerable<Peep> GetPeeps(this Regime r, Data data)
    {
        return r.GetPolys(data).Where(p => p.HasPeep(data))
            .Select(p => p.GetPeep(data));
    }

    public static int GetPopulation(this Regime r, Data data)
    {
        return r.GetPeeps(data).Sum(p => p.Size);
    }

    public static float GetPowerScore(this Regime r, Data data)
    {
        var fromPop = r.GetPopulation(data);
        var fromIndustry = r.Store.Get(data.Models.Flows.IndustrialPower);
        return fromPop + fromIndustry;
    }

    public static Alliance GetAlliance(this Regime r, Data data)
    {
        return data.Society.AllianceAux.RegimeAlliances[r];
    }

    public static bool IsAllied(this Regime r0, Regime r1, Data data)
    {
        return r0.GetAlliance(data).Members.Contains(r1);
    }
}
