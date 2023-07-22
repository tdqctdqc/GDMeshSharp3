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
        var p = key.Data.Handles.Proposals[ProposalId];
        p.CleanUp(key);
        key.Data.Handles.Proposals.Remove(ProposalId);
    }

    public override bool Valid(Data data)
    {
        return data.Handles.Proposals.ContainsKey(ProposalId);
    }
}
