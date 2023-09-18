using System;
using System.Collections.Generic;
using System.Linq;
using MessagePack;

public class Alliance : Entity
{
    public EntityRef<Regime> Leader { get; private set; }
    public EntRefCol<Regime> Members { get; private set; }
    public EntRefCol<Alliance> Rivals { get; private set; }
    public EntRefCol<Alliance> AtWar { get; private set; }
    public HashSet<int> ProposalIds { get; private set; }
    public IEnumerable<Proposal> Proposals(Data data) =>
        ProposalIds.Select(id => data.Society.Proposals.Proposals[id]);
    public static Alliance Create(Regime founder, CreateWriteKey key)
    {
        var members = EntRefCol<Regime>.Construct(nameof(Members), -1,
            new HashSet<int>{founder.Id}, key.Data);
        var enemies = EntRefCol<Alliance>.Construct(nameof(Rivals), -1,
            new HashSet<int>{}, key.Data);
        var atWar = EntRefCol<Alliance>.Construct(nameof(AtWar), -1,
            new HashSet<int>{}, key.Data);
        var proposals = new HashSet<int>();
        
        var a = new Alliance(founder.MakeRef(), members, enemies, atWar,
            proposals,
            -1);
        
        key.Create(a);
        return a;
    }
    [SerializationConstructor] private Alliance(EntityRef<Regime> leader,
        EntRefCol<Regime> members, 
        EntRefCol<Alliance> rivals, 
        EntRefCol<Alliance> atWar, 
        HashSet<int> proposalIds,
        int id) : base(id)
    {
        Leader = leader;
        Members = members;
        ProposalIds = proposalIds;
        Rivals = rivals;
        AtWar = atWar;
    }
}
