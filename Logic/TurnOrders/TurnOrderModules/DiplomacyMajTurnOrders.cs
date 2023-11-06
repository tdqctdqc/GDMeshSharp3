using System;
using System.Collections.Generic;
using System.Linq;

public class DiplomacyMajTurnOrders : TurnOrderModule
{
    public List<Proposal> ProposalsMade { get; private set; }
    public static DiplomacyMajTurnOrders Construct()
    {
        return new DiplomacyMajTurnOrders(new List<Proposal>());
    }
    public DiplomacyMajTurnOrders(List<Proposal> proposalsMade)
    {
        ProposalsMade = proposalsMade;
    }
}
