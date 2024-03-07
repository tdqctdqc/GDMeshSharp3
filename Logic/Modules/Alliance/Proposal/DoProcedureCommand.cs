
using System;

public class DoProcedureCommand : Command
{
    public Procedure Procedure { get; private set; }
    public DoProcedureCommand(
        Procedure procedure,
        Guid commandingPlayerGuid) 
        : base(commandingPlayerGuid)
    {
        Procedure = procedure;
    }

    public override void Enact(LogicWriteKey key)
    {
        key.SendMessage(Procedure);
    }

    public override bool Valid(Data data)
    {
        return Procedure.Valid(data);
    }
}