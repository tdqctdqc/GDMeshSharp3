
using System;
using System.Collections.Generic;

public class ReceiveProposalDecisionsModule : LogicModule
{
    public override void Calculate(List<RegimeTurnOrders> orders, LogicWriteKey key)
    {
        ReceiveProposalDecisions(orders, key);
    }
    private void ReceiveProposalDecisions(List<RegimeTurnOrders> orders, LogicWriteKey key)
    {
        foreach (var turnOrders in orders)
        {
            if (turnOrders is MinorTurnOrders m == false) throw new Exception();
            var regime = m.Regime.RefId;
            foreach (var kvp in m.Diplomacy.ProposalDecisions)
            {
                var decision = new DecideOnProposalProcedure(regime, kvp.Value,
                    kvp.Key);
                key.SendMessage(decision);
            }
        }
    }
}