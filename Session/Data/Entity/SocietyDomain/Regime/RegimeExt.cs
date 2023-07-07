
using System.Collections.Generic;
using System.Linq;

public static class RegimeExt
{
    public static RegimeRelation RelationWith(this Regime r1, Regime r2, Data data)
    {
        return data.Society.RelationAux.ByRegime[r1.Id, r2.Id];
    }

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

    public static IEnumerable<PolyPeep> GetPeeps(this Regime r, Data data)
    {
        return r.Polygons.Entities(data).SelectWhere(p => p.HasPeep(data))
            .Select(p => p.GetPeep(data));
    }

    public static int GetPopulation(this Regime r, Data data)
    {
        return r.GetPeeps(data).Sum(p => p.Size);
    }
}
