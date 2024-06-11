
using System;

public class CancelMakeProjectCommand : Command
{
    public ERef<Regime> Regime { get; private set; }
    public int MakeProjectId { get; private set; }
    public CancelMakeProjectCommand(
        ERef<Regime> regime,
        int makeProjectId,
        Guid commandingPlayerGuid) : base(commandingPlayerGuid)
    {
        Regime = regime;
        MakeProjectId = makeProjectId;
    }

    public override bool Valid(Data data, out string error)
    {
        error = "";
        return true;
    }

    public override void Enact(LogicWriteKey key)
    {
        var proc = new CancelMakeProjectProcedure(Regime, MakeProjectId);
        key.SendMessage(proc);
    }
}