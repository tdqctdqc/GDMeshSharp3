using System;
using System.Collections.Generic;
using System.Linq;

public class DiplomacyOrders : TurnOrderModule
{
    public List<Proposal> ProposalsMade { get; private set; }
    public Dictionary<int, bool> ProposalDecisions { get; private set; }
    public static DiplomacyOrders Construct()
    {
        return new DiplomacyOrders(new List<Proposal>(), new Dictionary<int, bool>());
    }
    public DiplomacyOrders(List<Proposal> proposalsMade, Dictionary<int, bool> proposalDecisions)
    {
        ProposalDecisions = proposalDecisions;
        ProposalsMade = proposalsMade;
    }
}
