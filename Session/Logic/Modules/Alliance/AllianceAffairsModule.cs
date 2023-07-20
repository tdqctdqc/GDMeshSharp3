using System;
using System.Collections.Generic;
using System.Linq;
using Godot;

public class AllianceAffairsModule : LogicModule
{
    private IdDispenser _id = new IdDispenser();
    public override LogicResults Calculate(List<TurnOrders> orders, Data data)
    {
        var res = new LogicResults();
        ResolveDiplomacyProposals(data, res);
        ResolveAllianceProposals(data, res);
        UpdateProposalPriorities(data, res);
        AddProposals(orders, res, data);
        return res;
    }
    private void ResolveDiplomacyProposals(Data data, LogicResults res)
    {
        var proposals = data.Society.Alliances.Entities
            .SelectMany(a => a.DiplomacyProposals).ToHashSet();
        foreach (var proposal in proposals)
        {
            var alliance0 = data.Society.Alliances[proposal.Alliance0];
            var alliance1 = data.Society.Alliances[proposal.Alliance1];
            var decision = allianceInFavor(alliance0) && allianceInFavor(alliance1);
            var resolve = new ResolveDipProposalProc(decision, proposal.Id, alliance0.Id, alliance1.Id);
            res.Procedures.Add(resolve);
            
            bool allianceInFavor(Alliance alliance)
            {
                var inFavor = proposal.InFavor.Where(f => alliance.Members.RefIds.Contains(f));
                var against = proposal.Against.Where(f => alliance.Members.RefIds.Contains(f));
                var forWeight = proposal.InFavor.Sum(f => alliance.GetWeightInAlliance(data.Society.Regimes[f], data));
                var againstWeight = proposal.Against.Sum(f => alliance.GetWeightInAlliance(data.Society.Regimes[f], data));
                return forWeight > againstWeight;
            }
        }
    }
    private void ResolveAllianceProposals(Data data, LogicResults res)
    {
        foreach (var alliance in data.Society.Alliances.Entities)
        {
            if (alliance.AllianceProposals.Count == 0) continue;
            var proposal = alliance.AllianceProposals.OrderBy(p => p.Priority).First();
            var forWeight = proposal.InFavor.Sum(f => alliance.GetWeightInAlliance(data.Society.Regimes[f], data));
            var againstWeight = proposal.Against.Sum(f => alliance.GetWeightInAlliance(data.Society.Regimes[f], data));
            var decision = forWeight > againstWeight;
            var resolve = new ResolveAllianceProposalProcedure(decision, proposal.Id, alliance.Id);
            res.Procedures.Add(resolve);
        }
    }
    private void UpdateProposalPriorities(Data data, LogicResults res)
    {
        var proc = UpdateAllianceProposalPrioritiesProc.Construct();
        foreach (var alliance in data.Society.Alliances.Entities)
        {
            foreach (var proposal in alliance.AllianceProposals)
            {
                var newPriority = proposal.Priority + alliance.GetWeightInAlliance(proposal.Proposer.Entity(data), data);
                proc.AllianceIds.Add(alliance.Id);
                proc.ProposalIds.Add(proposal.Id);
                proc.NewPriorities.Add(newPriority);
            }
        }
        res.Procedures.Add(proc);
    }

    private HashSet<int> _allProposals = new HashSet<int>();
    
    private void AddProposals(List<TurnOrders> orders, LogicResults res, Data data)
    {
        var tick = data.BaseDomain.GameClock.Tick;
        foreach (var turnOrders in orders)
        {
            if (turnOrders is MajorTurnOrders m == false) throw new Exception();
            if (turnOrders.Tick != tick) throw new Exception();
            var regime = turnOrders.Regime.Entity(data);
            foreach (var allianceProposal in m.DiplomacyOrders.AllianceProposals)
            {
                var id = _id.GetID();
                if (_allProposals.Contains(id)) throw new Exception();
                _allProposals.Add(id);
                allianceProposal.SetId(id);
                var proc = MakeProposalProcedure.Construct(allianceProposal);
                res.Procedures.Add(proc);
            }
            foreach (var diplomacyProposal in m.DiplomacyOrders.DiplomacyProposals)
            {
                var proc = MakeProposalProcedure.Construct(diplomacyProposal);
                res.Procedures.Add(proc);
            }
        }
    }
}
