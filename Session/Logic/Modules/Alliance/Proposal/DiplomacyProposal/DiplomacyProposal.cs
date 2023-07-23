using System;
using System.Collections.Generic;
using System.Linq;

public abstract class DiplomacyProposal : Proposal
{
    public int Alliance0 { get; protected set; }
    public int Alliance1 { get; protected set; }

    protected DiplomacyProposal(int alliance0, int alliance1, int id, EntityRef<Regime> proposer, HashSet<int> inFavor, 
        HashSet<int> against, float priority) 
        : base(id, proposer, inFavor, against, priority)
    {
        Alliance0 = alliance0;
        Alliance1 = alliance1;
    }
    
    public override void Propose(ProcedureWriteKey key)
    {
        var holder = key.Data.Get<Holder<Proposal>>(Id);
        if (key.Data.EntitiesById.ContainsKey(Alliance0) 
            && key.Data.EntitiesById.ContainsKey(Alliance1))
        {
            var alliance0 = key.Data.Get<Alliance>(Alliance0);
            alliance0.Proposals.Add(holder, key);
        
            var alliance1 = key.Data.Get<Alliance>(Alliance1);
            alliance1.Proposals.Add(holder, key);
        }
    }

    public override void CleanUp(ProcedureWriteKey key)
    {
        var holder = key.Data.Get<Holder<Proposal>>(Id);
        if (key.Data.EntitiesById.ContainsKey(Alliance0))
        {
            var alliance0 = key.Data.Get<Alliance>(Alliance0);
            alliance0.Proposals.Remove(holder, key);
        }
        if (key.Data.EntitiesById.ContainsKey(Alliance1))
        {
            var alliance1 = key.Data.Get<Alliance>(Alliance1);
            alliance1.Proposals.Remove(holder, key);
        }
    }
    
    public override float GetPriorityGrowth(Data data)
    {
        var alliance = Proposer.Entity(data).GetAlliance(data);
        var res = alliance.GetPowerScore(data);
        return res;
    }
    
    public override TriBool GetResolution(Data data)
    {
        if (data.EntitiesById.ContainsKey(Alliance0) == false || data.EntitiesById.ContainsKey(Alliance1) == false)
        {
            return TriBool.False;
        }
        var a0 = allianceInFavor(data.Get<Alliance>(Alliance0));
        var a1 = allianceInFavor(data.Get<Alliance>(Alliance1));
        return a0.And(a1);
        TriBool allianceInFavor(Alliance alliance)
        {
            var inFavor = InFavor.Where(f => alliance.Members.RefIds.Contains(f));
            var against = Against.Where(f => alliance.Members.RefIds.Contains(f));
            var undecided = alliance.Members.RefIds.Except(inFavor).Except(against);
            var forWeight = inFavor.Sum(f => alliance.GetWeightInAlliance(data.Get<Regime>(f), data));
            var againstWeight = against.Sum(f => alliance.GetWeightInAlliance(data.Get<Regime>(f), data));
            var undecidedWeight = undecided.Sum(f => alliance.GetWeightInAlliance(data.Get<Regime>(f), data));
            if (AllianceUndecided(alliance, data))
            {
                return TriBool.Undecided;
            }
            return new TriBool(forWeight > againstWeight);
        }
    }

    public override bool Undecided(Data data)
    {
        return AllianceUndecided(data.Get<Alliance>(Alliance0), data)
            || AllianceUndecided(data.Get<Alliance>(Alliance1), data);
    }
}
