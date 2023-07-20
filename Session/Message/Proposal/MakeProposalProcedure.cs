using System;
using System.Collections.Generic;
using System.Linq;
using MessagePack;

public class MakeProposalProcedure : Procedure
{
    public PortablePolymorph<Proposal> Proposal { get; private set; }
    public static MakeProposalProcedure Construct(Proposal proposal)
    {
        return new MakeProposalProcedure(PortablePolymorph<Proposal>.Construct(proposal));
    }
    [SerializationConstructor] private MakeProposalProcedure(PortablePolymorph<Proposal> proposal)
    {
        Proposal = proposal;
    }

    public override bool Valid(Data data)
    {
        return true;
    }

    public override void Enact(ProcedureWriteKey key)
    {
        var proposal = Proposal.Get();
        proposal.Propose(key);
    }
}
