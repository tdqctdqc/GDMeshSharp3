
using System;

public class MakeProposalCommand : Command
{
    public Proposal Proposal { get; private set; }

    public MakeProposalCommand(Guid commandingPlayerGuid, Proposal proposal) : base(commandingPlayerGuid)
    {
        Proposal = proposal;
    }

    public override bool Valid(Data data)
    {
        return true;
    }

    public override void Enact(ProcedureWriteKey key)
    {
        MakeProposalProcedure.Construct(Proposal, key.Data).Enact(key);
    }
}