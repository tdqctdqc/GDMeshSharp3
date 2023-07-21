using System;
using System.Collections.Generic;
using System.Linq;

public abstract class DiplomacyProposal : Proposal
{
    public int Alliance0 { get; private set; }
    public int Alliance1 { get; private set; }

    protected DiplomacyProposal(int alliance0, int alliance1, int id, EntityRef<Regime> proposer, HashSet<int> inFavor, 
        HashSet<int> against, float priority) 
        : base(id, proposer, inFavor, against, priority)
    {
        Alliance0 = alliance0;
        Alliance1 = alliance1;
    }
    
    public override void Propose(ProcedureWriteKey key)
    {
        if (key.Data.Entities.ContainsKey(Alliance0) 
            && key.Data.Entities.ContainsKey(Alliance1))
        {
            var alliance0 = key.Data.Society.Alliances[Alliance0];
            alliance0.Proposals.Add(Id, this);
        
            var alliance1 = key.Data.Society.Alliances[Alliance1];
            alliance1.Proposals.Add(Id, this);
        }
    }

    protected override void CleanUp(ProcedureWriteKey key)
    {
        if (key.Data.Entities.ContainsKey(Alliance0))
        {
            var alliance0 = key.Data.Society.Alliances[Alliance0];
            alliance0.Proposals.Remove(Id);
        }
        if (key.Data.Entities.ContainsKey(Alliance1))
        {
            var alliance1 = key.Data.Society.Alliances[Alliance1];
            alliance1.Proposals.Remove(Id);
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
        if (data.Entities.ContainsKey(Alliance0) == false || data.Entities.ContainsKey(Alliance1) == false)
        {
            return TriBool.False;
        }
        var a0 = allianceInFavor(data.Society.Alliances[Alliance0]);
        var a1 = allianceInFavor(data.Society.Alliances[Alliance1]);
        return a0.Combine(a1);
        TriBool allianceInFavor(Alliance alliance)
        {
            var inFavor = InFavor.Where(f => alliance.Members.RefIds.Contains(f));
            var against = Against.Where(f => alliance.Members.RefIds.Contains(f));
            var undecided = alliance.Members.RefIds.Except(inFavor).Except(against);
            var forWeight = inFavor.Sum(f => alliance.GetWeightInAlliance(data.Society.Regimes[f], data));
            var againstWeight = against.Sum(f => alliance.GetWeightInAlliance(data.Society.Regimes[f], data));
            var undecidedWeight = undecided.Sum(f => alliance.GetWeightInAlliance(data.Society.Regimes[f], data));
            if (undecidedWeight > forWeight && undecidedWeight > againstWeight)
            {
                return TriBool.Undecided;
            }
            return new TriBool(forWeight > againstWeight);
        }
    }
}
