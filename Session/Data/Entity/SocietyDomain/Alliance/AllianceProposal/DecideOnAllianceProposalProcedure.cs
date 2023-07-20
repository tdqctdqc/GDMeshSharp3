using System;
using System.Collections.Generic;
using System.Linq;

public class DecideOnAllianceProposalProcedure : Procedure
{
    public int ProposalId { get; private set; }
    public bool Decision { get; private set; }
    public int Regime { get; private set; }

    public DecideOnAllianceProposalProcedure(int proposalId, bool decision, int regime)
    {
        ProposalId = proposalId;
        Decision = decision;
        Regime = regime;
    }

    public override bool Valid(Data data)
    {
        return true;
    }

    public override void Enact(ProcedureWriteKey key)
    {
        var proposal = key.Data.Society.Regimes[Regime].GetAlliance(key.Data)
            .AllianceProposals.First(p => p.Id == ProposalId);
        proposal.Abstain.Remove(Regime);
        proposal.InFavor.Remove(Regime);
        proposal.Against.Remove(Regime);

        if (Decision)
        {
            proposal.InFavor.Add(Regime);
        }
        else
        {
            proposal.Against.Add(Regime);
        }
    }
}
