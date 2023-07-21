using System;
using System.Collections.Generic;
using System.Linq;
using Godot;

public class ResolveProposalProcedure : Procedure
{
    public bool Accepted { get; private set; }
    public int ProposalId { get; private set; }
    public ResolveProposalProcedure(bool accepted, int proposalId)
    {
        Accepted = accepted;
        ProposalId = proposalId;
    }
    public override void Enact(ProcedureWriteKey key)
    {
        if (key.Data.Handles.Proposals.ContainsKey(ProposalId) == false) return;
        var proposal = key.Data.Handles.Proposals[ProposalId];
        proposal.Resolve(Accepted, key);
        key.Data.Handles.Proposals.Remove(proposal.Id);
    }
    public override bool Valid(Data data)
    {
        return true;
    }
}
