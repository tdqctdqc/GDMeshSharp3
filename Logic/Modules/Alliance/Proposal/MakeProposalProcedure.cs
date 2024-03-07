using System;
using System.Collections.Generic;
using System.Linq;
using Godot;
using MessagePack;

public class MakeProposalProcedure : Procedure
{
    public Proposal Proposal { get; private set; }
    public static MakeProposalProcedure Construct(Proposal p,
        LogicWriteKey key)
    {
        p.SetId(key.Data.IdDispenser.TakeId());
        return new MakeProposalProcedure(p);
    }
    [SerializationConstructor] 
    private MakeProposalProcedure(Proposal proposal)
    {
        Proposal = proposal;
    }
    public override void Enact(ProcedureWriteKey key)
    {
        if (key.Data.Society.Proposals.Proposals.ContainsKey(Proposal.Id))
        {
            var already = key.Data.Society.Proposals.Proposals[Proposal.Id];
            throw new Exception($"Can't add {Proposal.GetType()}" +
                                $" already proposal " + Proposal.Id);
        }
        else
        {
            key.Data.Society.Proposals.Proposals.Add(Proposal.Id, Proposal);
        }
    }

    public override bool Valid(Data data)
    {
        return Proposal.Valid(data);
    }
}
