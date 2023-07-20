using System;
using System.Collections.Generic;
using System.Linq;

public class DecideOnDipProposalProc : Procedure
{
    public int ProposalId { get; private set; }
    public bool Decision { get; private set; }
    public int Regime { get; private set; }

    public DecideOnDipProposalProc(int proposalId, bool decision, int regime)
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
        var regime = key.Data.Society.Regimes[Regime];
        var prop = regime.GetAlliance(key.Data).DiplomacyProposals.First(p => p.Id == ProposalId);
        prop.Abstain.Remove(Regime);
        prop.InFavor.Remove(Regime);
        prop.Against.Remove(Regime);
        if (Decision)
        {
            prop.InFavor.Add(Regime);
        }
        else
        {
            prop.Against.Add(Regime);
        }
    }
}
