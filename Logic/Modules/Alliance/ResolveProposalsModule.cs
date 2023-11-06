
using System.Collections.Generic;
using System.Linq;

public class ResolveProposalsModule : LogicModule
{
    public override void Calculate(List<RegimeTurnOrders> orders, LogicWriteKey key)
    {
        var proposals = key.Data.Society.Proposals.Proposals.Values.ToList();
        ResolveProposals(proposals, key);
    }
    private void ResolveProposals(List<Proposal> proposals, LogicWriteKey key)
    {
        var resolved = new HashSet<int>();
        var readyProposals = proposals
            .Where(p => resolved.Contains(p.Id) == false)
            .Where(p => p.Valid(key.Data))
            .Where(p => p.Undecided(key.Data) == false)
            .ToHashSet();
        if (readyProposals.Count() > 0)
        {
            var proposal = readyProposals.First();
            resolved.Add(proposal.Id);
            var decision = proposal.GetResolution(key.Data);
            var resolve = new ResolveProposalProcedure(decision.IsTrue(), proposal.Id);
            key.SendMessage(resolve);
        }
    }
}