
using System.Collections.Generic;
using System.Linq;

public static class RegimeExt
{
    public static bool IsPlayerRegime(this Regime r, Data data)
    {
        return data.BaseDomain.PlayerAux.ByRegime.ContainsKey(r);
    }
    public static bool IsLocalPlayerRegime(this Regime r, Data data)
    {
        return data.BaseDomain.PlayerAux.LocalPlayer.Regime.Entity(data) == r;
    }
    public static Player GetPlayer(this Regime r, Data data)
    {
        return data.BaseDomain.PlayerAux.ByRegime[r];
    }

    public static IEnumerable<MapPolygon> GetPolys(this Regime r, Data data)
    {
        return data.Planet.PolygonAux.PolysByRegime[r];
    }
    public static IEnumerable<PolyPeep> GetPeeps(this Regime r, Data data)
    {
        return r.GetPolys(data).SelectWhere(p => p.HasPeep(data))
            .Select(p => p.GetPeep(data));
    }

    public static int GetPopulation(this Regime r, Data data)
    {
        return r.GetPeeps(data).Sum(p => p.Size);
    }

    public static float GetPowerScore(this Regime r, Data data)
    {
        var fromPop = r.GetPopulation(data);
        var fromIndustry = r.Flows[FlowManager.IndustrialPower].FlowIn;
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
