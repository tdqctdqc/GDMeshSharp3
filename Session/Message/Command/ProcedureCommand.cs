using System;
using System.Collections.Generic;
using System.Linq;

public class ProcedureCommand : Command
{
    public Procedure Procedure { get; private set; }

    public ProcedureCommand(Procedure procedure)
    {
        Procedure = procedure;
    }
    public override void Enact(HostWriteKey key, Action<Procedure> queueProcedure)
    {
        queueProcedure(Procedure);
    }

    public override bool Valid(Data data)
    {
        return Procedure.Valid(data);
    }
}
