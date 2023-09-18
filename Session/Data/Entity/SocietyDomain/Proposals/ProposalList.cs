
using System.Collections.Generic;
using MessagePack;

public class ProposalList : Entity
{
    public Dictionary<int, Proposal> Proposals { get; private set; }

    public static ProposalList Create(GenWriteKey key)
    {
        var p = new ProposalList(-1, new Dictionary<int, Proposal>());
        key.Create(p);
        return p;
    }
    [SerializationConstructor] private ProposalList(int id, Dictionary<int, Proposal> proposals) : base(id)
    {
        Proposals = proposals;
    }
}