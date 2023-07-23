using System;
using System.Collections.Generic;
using System.Linq;
using Godot;
using MessagePack;

public class MakeProposalProcedure : Procedure
{
    public PortablePolymorph<Proposal> Proposal { get; private set; }
    public static MakeProposalProcedure Construct(Proposal p, Data d)
    {
        return new MakeProposalProcedure(PortablePolymorph<Proposal>.Construct(p, d));
    }
    [SerializationConstructor] private MakeProposalProcedure(PortablePolymorph<Proposal> proposal)
    {
        Proposal = proposal;
    }
    public override void Enact(ProcedureWriteKey key)
    {
        var p = Proposal.Get(key.Data);
        var holder = Holder<Proposal>.Create(p, key);
        p.SetId(holder.Id);
        if (key.Data.Handles.Proposals.ContainsKey(p.Id))
        {
            var already = key.Data.Handles.Proposals[p.Id];
            throw new Exception($"Can't add {p.GetType()} already proposal " + already.GetType());
        }
        else
        {
            key.Data.Handles.Proposals.Add(p.Id, p);
            p.Propose(key);
        }
        
    }

    public override bool Valid(Data data)
    {
        return true;
    }
}
