
using System.Collections.Generic;
using System.Linq;
using Godot;

public class ResolveProposalsModule : LogicModule
{
    public override void Calculate(List<RegimeTurnOrders> orders, LogicWriteKey key)
    {
        ResolveProposals(key);
    }
    private void ResolveProposals(LogicWriteKey key)
    {
        var proposals = key.Data.Society.Proposals
            .Proposals.Values;

        var readyProposals = proposals
            .Where(p => p.Valid(key.Data))
            .Where(p => p.Undecided(key.Data) == false);
        
        foreach (var proposal in readyProposals)
        {
            var decision = proposal.GetResolution(key.Data);
            var resolve = new ResolveProposalProcedure(decision.IsTrue(), proposal.Id);
            key.SendMessage(resolve);
        }
    }
}