using System;
using System.Collections.Generic;
using System.Linq;
using MessagePack;

public class Alliance : Entity
{
    public ERef<Regime> Leader { get; private set; }
    public ERefSetCallback<Regime> Members { get; private set; }
    public IEnumerable<Proposal> PendingProposals(Data data) =>
        data.Society.Proposals
            .Proposals.Values.Where(p => p.Target.RefId == Id);
    public static Alliance Create(Regime founder, GenKey key)
    {
        var id = key.Data.IdDispenser.TakeId();
        var members = ERefSetCallback<Regime>.Construct(
            new HashSet<ERef<Regime>>{founder.MakeRef()});
        
        var a = new Alliance(founder.MakeRef(), members,
            id);
        
        key.Create(a);
        return a;
    }
    [SerializationConstructor] private Alliance(ERef<Regime> leader,
        ERefSetCallback<Regime> members, 
        int id) : base(id)
    {
        Leader = leader;
        Members = members;
        Members.AddIndexerCallbacks(this, 
            d => d.Society.AllianceAux.RegimeAlliances);
    }

    public override void CleanUp(ProcedureKey key)
    {
        if (Members.Count() > 0) throw new Exception();
        key.GetData().Society.DiploGraph.RemoveAlliance(this, key);
    }
}
