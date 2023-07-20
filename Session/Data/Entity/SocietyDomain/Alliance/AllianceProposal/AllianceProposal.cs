using System;
using System.Collections.Generic;
using System.Linq;
using MessagePack;

public abstract class AllianceProposal : Proposal
{
    public int Alliance { get; private set; }
    public float Priority { get; private set; }
    
    [SerializationConstructor] protected AllianceProposal(int id, EntityRef<Regime> proposer, 
        int alliance, HashSet<int> inFavor, HashSet<int> against, HashSet<int> abstain, float priority) 
        : base(id, proposer, alliance, inFavor, against, abstain)
    {
        Alliance = alliance;
        Priority = priority;
    }

    
    public void UpdatePriority(float newPriority, ProcedureWriteKey key)
    {
        Priority = newPriority;
    }

    public override void Propose(ProcedureWriteKey key)
    {
        var alliance = key.Data.Society.Alliances[Alliance];
        alliance.AllianceProposals.Add(this);
    }
}