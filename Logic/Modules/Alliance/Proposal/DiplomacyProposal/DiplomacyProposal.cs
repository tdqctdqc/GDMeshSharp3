using System;
using System.Collections.Generic;
using System.Linq;

public abstract class DiplomacyProposal : Proposal
{
    public int Alliance0 { get; protected set; }
    public int Alliance1 { get; protected set; }

    protected DiplomacyProposal(int alliance0, int alliance1, 
        int id, EntityRef<Regime> proposer,
        HashSet<int> inFavor, HashSet<int> against) 
        : base(id, proposer, new HashSet<int>{alliance0, alliance1}, 
            inFavor, against)
    {
        Alliance0 = alliance0;
        Alliance1 = alliance1;
    }
    
    public override void Propose(ProcedureWriteKey key)
    {
        if (key.Data.EntitiesById.ContainsKey(Alliance0) 
            && key.Data.EntitiesById.ContainsKey(Alliance1))
        {
            var alliance0 = key.Data.Get<Alliance>(Alliance0);
            alliance0.ProposalIds.Add(Id);
        
            var alliance1 = key.Data.Get<Alliance>(Alliance1);
            alliance1.ProposalIds.Add(Id);
            
            var proposer = this.Proposer.RefId;
            InFavor.Add(proposer);
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
        if (data.EntitiesById.ContainsKey(Alliance0) == false 
            || data.EntitiesById.ContainsKey(Alliance1) == false)
        {
            return TriBool.False;
        }
        var a0 = AllianceInFavor(data.Get<Alliance>(Alliance0), data);
        var a1 = AllianceInFavor(data.Get<Alliance>(Alliance1), data);
        return a0.And(a1);
    }
}
