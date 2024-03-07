
using System;

public class MakeProposalCommand : Command
{
    public Proposal Proposal { get; private set; }
    public MakeProposalCommand(Guid commandingPlayerGuid, Proposal proposal) 
        : base(commandingPlayerGuid)
    {
        Proposal = proposal;
    }

    public override bool Valid(Data data)
    {
        return true;
    }

    public override void Enact(LogicWriteKey key)
    {
        key.SendMessage(MakeProposalProcedure.Construct(Proposal, key));
    }
}