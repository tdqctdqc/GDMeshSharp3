using System;
using System.Collections.Generic;
using System.Linq;

public class DiplomacyOrders : TurnOrderModule
{
    public List<DiplomacyProposal> DiplomacyProposals { get; private set; }
    public List<AllianceProposal> AllianceProposals { get; private set; }
    public static DiplomacyOrders Construct()
    {
        return new DiplomacyOrders(new List<DiplomacyProposal>(), new List<AllianceProposal>());
    }
    public DiplomacyOrders(List<DiplomacyProposal> diplomacyProposals, List<AllianceProposal> allianceProposal)
    {
        DiplomacyProposals = diplomacyProposals;
        AllianceProposals = allianceProposal;
    }
}
