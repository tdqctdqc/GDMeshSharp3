using System;
using System.Collections.Generic;
using System.Linq;
using Godot;

public class ProposalsModule : LogicModule
{
    public override void Calculate(List<RegimeTurnOrders> orders, Data data,
        Action<Message> sendMessage)
    {
        var proposals = data.Society.Proposals.Proposals.Values.ToList();
        ReceiveProposalDecisions(orders, data, sendMessage);
        ResolveProposals(proposals, data, sendMessage);
        AddProposals(orders, sendMessage, data);
        RemoveInvalidProposals(data, sendMessage);
    }

    
    private void ReceiveProposalDecisions(List<RegimeTurnOrders> orders, Data data, Action<Message> sendMessage)
    {
        foreach (var turnOrders in orders)
        {
            if (turnOrders is MajorTurnOrders m == false) throw new Exception();
            var regime = m.Regime.RefId;
            foreach (var kvp in m.Diplomacy.ProposalDecisions)
            {
                var decision = new DecideOnProposalProcedure(regime, kvp.Value,
                    kvp.Key);
                sendMessage(decision);
            }
        }
    }
    private void ResolveProposals(List<Proposal> proposals, Data data, Action<Message> sendMessage)
    {
        var resolved = new HashSet<int>();
        var readyProposals = proposals
            .Where(p => resolved.Contains(p.Id) == false)
            .Where(p => p.Valid(data))
            .Where(p => p.Undecided(data) == false)
            .ToHashSet();
        if (readyProposals.Count() > 0)
        {
            var proposal = readyProposals.First();
            resolved.Add(proposal.Id);
            var decision = proposal.GetResolution(data);
            var resolve = new ResolveProposalProcedure(decision.IsTrue(), proposal.Id);
            sendMessage(resolve);
        }
    }
    
    private void RemoveInvalidProposals(Data data, Action<Message> sendMessage)
    {
        var invalids = data.Society.Proposals.Proposals.Values.Where(p => p.Valid(data) == false);
        foreach (var invalid in invalids)
        {
            sendMessage(new CancelProposalProcedure(invalid.Id));
        }
    }
    private void AddProposals(List<RegimeTurnOrders> orders, Action<Message> sendMessage, Data data)
    {
        var tick = data.BaseDomain.GameClock.Tick;
        foreach (var turnOrders in orders)
        {
            if (turnOrders is MajorTurnOrders m == false) throw new Exception();
            if (turnOrders.Tick != tick) throw new Exception();
            var regime = turnOrders.Regime.Entity(data);
            foreach (var proposal in m.Diplomacy.ProposalsMade)
            {
                var id = data.IdDispenser.TakeId();
                proposal.SetId(id);
                var proc = MakeProposalProcedure.Construct(proposal, data);
                sendMessage(proc);
            }
        }
    }
}
