
using System;
using Godot;

public class ChooseRegimeCommand : Command
{
    public ERef<Regime> Regime { get; private set; }
    public ChooseRegimeCommand(ERef<Regime> regime, Guid commandingPlayerGuid) 
        : base(commandingPlayerGuid)
    {
        Regime = regime;
    }

    public override void Enact(LogicWriteKey key)
    {
        var proc = new SetPlayerRegimeProcedure(Regime, CommandingPlayerGuid);
        key.SendMessage(proc);
    }

    public override bool Valid(Data data, out string error)
    {
        if (Regime.Get(data).IsPlayerRegime(data))
        {
            error = "Regime already has player";
            return false;
        }

        error = "";
        return true;
    }
}
