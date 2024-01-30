using System;
using System.Collections.Generic;
using System.Linq;
using MessagePack;

public class Alliance : Entity
{
    public ERef<Regime> Leader { get; private set; }
    public ERefSet<Regime> Members { get; private set; }
    public HashSet<int> ProposalIds { get; private set; }
    public IEnumerable<Proposal> Proposals(Data data) =>
        ProposalIds.Select(id => data.Society.Proposals.Proposals[id]);
    public static Alliance Create(Regime founder, ICreateWriteKey key)
    {
        var id = key.Data.IdDispenser.TakeId();
        var members = ERefSet<Regime>.Construct(nameof(Members), id,
            new HashSet<int>{founder.Id}, key.Data);
        var proposals = new HashSet<int>();
        
        var a = new Alliance(founder.MakeRef(), members,
            proposals,
            id);
        
        key.Create(a);
        return a;
    }
    [SerializationConstructor] private Alliance(ERef<Regime> leader,
        ERefSet<Regime> members, 
        HashSet<int> proposalIds,
        int id) : base(id)
    {
        Leader = leader;
        Members = members;
        ProposalIds = proposalIds;
    }

    public override void CleanUp(StrongWriteKey key)
    {
        if (Members.Count() > 0) throw new Exception();
        key.Data.Society.DiploGraph.RemoveAlliance(this, key);
    }
}
