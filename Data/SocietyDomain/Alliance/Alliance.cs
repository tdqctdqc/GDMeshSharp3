using System;
using System.Collections.Generic;
using System.Linq;
using MessagePack;

public class Alliance : Entity
{
    public ERef<Regime> Leader { get; private set; }
    public ERefSet<Regime> Members { get; private set; }
    public IEnumerable<Proposal> PendingProposals(Data data) =>
        data.Society.Proposals
            .Proposals.Values.Where(p => p.Target.RefId == Id);
    public static Alliance Create(Regime founder, ICreateWriteKey key)
    {
        var id = key.Data.IdDispenser.TakeId();
        var members = ERefSet<Regime>.Construct(nameof(Members), id,
            new HashSet<int>{founder.Id}, key.Data);
        
        var a = new Alliance(founder.MakeRef(), members,
            id);
        
        key.Create(a);
        return a;
    }
    [SerializationConstructor] private Alliance(ERef<Regime> leader,
        ERefSet<Regime> members, 
        int id) : base(id)
    {
        Leader = leader;
        Members = members;
    }

    public override void CleanUp(StrongWriteKey key)
    {
        if (Members.Count() > 0) throw new Exception();
        key.Data.Society.DiploGraph.RemoveAlliance(this, key);
    }
}
