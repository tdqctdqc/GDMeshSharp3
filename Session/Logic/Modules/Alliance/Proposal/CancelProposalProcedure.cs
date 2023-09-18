using System;
using System.Collections.Generic;
using System.Linq;

public class CancelProposalProcedure : Procedure
{
    public int ProposalId { get; private set; }

    public CancelProposalProcedure(int proposalId)
    {
        ProposalId = proposalId;
    }

    public override void Enact(ProcedureWriteKey key)
    {
        var p = key.Data.Society.Proposals.Proposals[ProposalId];
        p.CleanUp(key);
        key.Data.Society.Proposals.Proposals.Remove(ProposalId);
    }

    public override bool Valid(Data data)
    {
        return data.Society.Proposals.Proposals.ContainsKey(ProposalId);
    }
}
