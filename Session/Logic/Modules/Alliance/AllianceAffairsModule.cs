using System;
using System.Collections.Generic;
using System.Linq;
using Godot;

public class AllianceAffairsModule : LogicModule
{
    private IdDispenser _id = new ();
    public override LogicResults Calculate(List<TurnOrders> orders, Data data)
    {
        var res = new LogicResults();
        ReceiveProposalDecisions(orders, data, res);
        UpdateProposalPriorities(data, res);
        ResolveProposals(orders, data, res);
        AddProposals(orders, res, data);
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
                var decision = new DecideOnProposalProcedure(regime, kvp.Value, kvp.Key);
                res.Messages.Add(decision);
            }
        }
    }
    private void ResolveProposals(List<TurnOrders> orders, Data data, LogicResults res)
    {
        var proposals = data.Society.Alliances.Entities
            .SelectMany(a => a.Proposals.Values).ToHashSet();
        foreach (var proposal in proposals)
        {
            var decision = proposal.GetResolution(data);
            if (decision.IsUndecided()) continue;
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
                var proc = MakeProposalProcedure.Construct(proposal, data);
                res.Messages.Add(proc);
            }
        }
    }
}
