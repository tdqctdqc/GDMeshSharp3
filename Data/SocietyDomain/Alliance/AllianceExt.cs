using System;
using System.Collections.Generic;
using System.Linq;

public static class AllianceExt
{
    public static float GetPowerScore(this Alliance a, Data data)
    {
        return a.Members.Items(data).Sum(r => r.GetPowerScore(data));
    }

    public static float GetWeightInAlliance(this Alliance a, Regime r, Data data)
    {
        var w = r.GetPowerScore(data);
        w *= w;
        if (a.Leader.RefId == r.Id) w *= 2;
        return w;
    }

    public static IEnumerable<Alliance> GetNeighborAlliances(this Alliance alliance, Data data)
    {
        return alliance.Members.Items(data)
            .SelectMany(r => r.GetCells(data))
            .SelectMany(p => p.GetNeighbors(data).Where(e => e.Controller.Fulfilled()))
            .Select(p => p.Controller.Get(data).GetAlliance(data))
            .Distinct()
            .Where(a => a != alliance);
    }

    public static bool IsRivals(this Alliance a, Alliance b, Data d)
    {
        return d.Society.DiploGraph.HasRelation(a, b, DiploRelation.Rivals);
    }
    public static bool IsAtWar(this Alliance a, Alliance b, Data d)
    {
        return d.Society.DiploGraph.HasRelation(a, b, DiploRelation.War);
    }

    public static IEnumerable<Alliance> GetRivals(this Alliance a, Data d)
    {
        return d.Society.DiploGraph.GetRelations(a, DiploRelation.Rivals, d);
    }
    public static IEnumerable<Alliance> GetAtWar(this Alliance a, Data d)
    {
        return d.Society.DiploGraph.GetRelations(a, DiploRelation.War, d);
    }

    public static void CreateRelation(this Alliance a, Alliance b, DiploRelation e, StrongWriteKey key)
    {
        key.Data.Society.DiploGraph.AddEdge(a, b, e, key);
    }
}
