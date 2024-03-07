using System;
using System.Collections.Generic;
using System.Linq;

public class DecideOnProposalProcedure : Procedure
{
    public bool Decision { get; private set; }
    public int ProposalId { get; private set; }
    public DecideOnProposalProcedure(bool decision, int proposalId)
    {
        Decision = decision;
        ProposalId = proposalId;
    }
    public override void Enact(ProcedureWriteKey key)
    {
        var p = key.Data.Society.Proposals.Proposals[ProposalId];
        p.Resolve(Decision, key);
    }
    public override bool Valid(Data data)
    {
        return data.Society.Proposals.Proposals.ContainsKey(ProposalId);
    }
}
