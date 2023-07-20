using System;
using System.Collections.Generic;
using System.Linq;

public abstract class DiplomacyProposal : Proposal
{
    public int Alliance0 { get; private set; }
    public int Alliance1 { get; private set; }

    protected DiplomacyProposal(int alliance0, int alliance1, int id, EntityRef<Regime> proposer, int alliance, HashSet<int> inFavor, 
        HashSet<int> against, HashSet<int> abstain) 
        : base(id, proposer, alliance, inFavor, against, abstain)
    {
        Alliance0 = alliance0;
        Alliance1 = alliance1;
    }

    public override void Propose(ProcedureWriteKey key)
    {
        var alliance0 = key.Data.Society.Alliances[Alliance0];
        alliance0.DiplomacyProposals.Add(this);
        
        var alliance1 = key.Data.Society.Alliances[Alliance1];
        alliance1.DiplomacyProposals.Add(this);
    }
}
