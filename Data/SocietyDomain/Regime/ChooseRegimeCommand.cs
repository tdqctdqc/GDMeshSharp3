
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

    public override bool Valid(Data data)
    {
        return Regime.Entity(data).IsPlayerRegime(data) == false;
    }
}
