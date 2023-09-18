using System;
using System.Collections.Generic;
using System.Linq;
using Godot;

public class AllianceAffairsModule : LogicModule
{
    public override LogicResults Calculate(List<TurnOrders> orders, Data data)
    {
        var res = new LogicResults();
        var proposals = data.Handles.Proposals.Values.ToList();
        ReceiveProposalDecisions(orders, data, res);
        UpdateProposalPriorities(data, res);
        ResolveProposals(proposals, data, res);
        AddProposals(orders, res, data);
        RemoveInvalidProposals(data, res);
        return res;
    }

    
    private void ReceiveProposalDecisions(List<TurnOrders> orders, Data data, LogicResults res)
    {
        foreach (var turnOrders in orders)
        {
            if (turnOrders is MajorTurnOrders m == false) throw new Exception();
            var regime = m.Regime.RefId;
            foreach (var kvp in m.DiplomacyOrders.ProposalDecisions)
            {
                var decision = new DecideOnProposalProcedure(regime, kvp.Value,
                    kvp.Key);
                res.Messages.Add(decision);
            }
        }
    }
    private void ResolveProposals(List<Proposal> proposals, Data data, LogicResults res)
    {
        var resolved = new HashSet<int>();
        var readyProposals = proposals
            .Where(p => resolved.Contains(p.Id) == false)
            .Where(p => p.Valid(data))
            .Where(p => p.Undecided(data) == false)
            .ToHashSet();
        if (readyProposals.Count() > 0)
        {
            var proposal = readyProposals
                .OrderByDescending(p => p.Priority).First();
            resolved.Add(proposal.Id);
            var decision = proposal.GetResolution(data);
            var resolve = new ResolveProposalProcedure(decision.IsTrue(), proposal.Id);
            res.Messages.Add(resolve);
        }
    }
    private void UpdateProposalPriorities(Data data, LogicResults res)
    {
        var proc = UpdateAllianceProposalPrioritiesProc.Construct();
        foreach (var proposal in data.Handles.Proposals.Values)
        {
            var growth = proposal.GetPriorityGrowth(data);
            proc.ProposalIds.Add(proposal.Id);
            proc.NewPriorities.Add(proposal.Priority + growth);
        }
        res.Messages.Add(proc);
    }
    private void RemoveInvalidProposals(Data data, LogicResults res)
    {
        var invalids = data.Handles.Proposals.Values.Where(p => p.Valid(data) == false);
        foreach (var invalid in invalids)
        {
            res.Messages.Add(new CancelProposalProcedure(invalid.Id));
        }
    }
    private void AddProposals(List<TurnOrders> orders, LogicResults res, Data data)
    {
        var tick = data.BaseDomain.GameClock.Tick;
        foreach (var turnOrders in orders)
        {
            if (turnOrders is MajorTurnOrders m == false) throw new Exception();
            if (turnOrders.Tick != tick) throw new Exception();
            var regime = turnOrders.Regime.Entity(data);
            foreach (var proposal in m.DiplomacyOrders.ProposalsMade)
            {
                proposal.SetId(data.IdDispenser.TakeId());
                var proc = MakeProposalProcedure.Construct(proposal, data);
                res.Messages.Add(proc);
            }
        }
    }
}
