using System;
using System.Collections.Generic;
using System.Linq;
using Godot;

public class ReceiveProposalsModule : LogicModule
{
    public override void Calculate(List<RegimeTurnOrders> orders, LogicWriteKey key)
    {
        AddProposals(orders, key);
        RemoveInvalidProposals(key);
    }
    private void RemoveInvalidProposals(LogicWriteKey key)
    {
        var invalids = key.Data.Society.Proposals.Proposals.Values.Where(p => p.Valid(key.Data) == false);
        foreach (var invalid in invalids)
        {
            key.SendMessage(new CancelProposalProcedure(invalid.Id));
        }
    }
    private void AddProposals(List<RegimeTurnOrders> orders, LogicWriteKey key)
    {
        var tick = key.Data.BaseDomain.GameClock.Tick;
        foreach (var turnOrders in orders)
        {
            if (turnOrders is MajorTurnOrders m == false) throw new Exception();
            if (turnOrders.Tick != tick) throw new Exception();
            var regime = turnOrders.Regime.Entity(key.Data);
            foreach (var proposal in m.Diplomacy.ProposalsMade)
            {
                var id = key.Data.IdDispenser.TakeId();
                proposal.SetId(id);
                var proc = MakeProposalProcedure.Construct(proposal, key.Data);
                key.SendMessage(proc);
            }
        }
    }
}
