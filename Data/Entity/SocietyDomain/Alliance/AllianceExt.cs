using System;
using System.Collections.Generic;
using System.Linq;

public static class AllianceExt
{
    public static IEnumerable<Proposal> GetProposals(this Alliance a, Data d)
    {
        return a.ProposalIds.Select(pId => d.Society.Proposals.Proposals[pId]);
    }
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

    public static IEnumerable<Alliance> GetNeighborAlliances(this Regime regime, Data data)
    {
        return regime.GetPolys(data)
            .SelectMany(p => p.Neighbors.Items(data).Where(e => e.OwnerRegime.Fulfilled()))
            .Select(p => p.OwnerRegime.Entity(data).GetAlliance(data)).Distinct();
    }
}
