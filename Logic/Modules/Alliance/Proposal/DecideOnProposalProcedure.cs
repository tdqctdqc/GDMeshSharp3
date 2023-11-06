using System;
using System.Collections.Generic;
using System.Linq;

public class DecideOnProposalProcedure : Procedure
{
    public int RegimeId { get; private set; }
    public bool Decision { get; private set; }
    public int ProposalId { get; private set; }
    public DecideOnProposalProcedure(int regimeId, bool decision, int proposalId)
    {
        RegimeId = regimeId;
        Decision = decision;
        ProposalId = proposalId;
    }
    public override void Enact(ProcedureWriteKey key)
    {
        var p = key.Data.Society.Proposals.Proposals[ProposalId];
        p.Decide(RegimeId, Decision, key);
    }
    public override bool Valid(Data data)
    {
        return data.Society.Proposals.Proposals.ContainsKey(ProposalId);
    }
}
