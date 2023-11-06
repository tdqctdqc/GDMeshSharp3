
using System.Collections.Generic;
using MessagePack;

public class DiplomacyMinTurnOrders : TurnOrderModule
{
    public Dictionary<int, bool> ProposalDecisions { get; private set; }

    public static DiplomacyMinTurnOrders Construct()
    {
        return new DiplomacyMinTurnOrders(new Dictionary<int, bool>());
    }
    [SerializationConstructor] protected DiplomacyMinTurnOrders(Dictionary<int, bool> proposalDecisions)
    {
        ProposalDecisions = proposalDecisions;
    }
}