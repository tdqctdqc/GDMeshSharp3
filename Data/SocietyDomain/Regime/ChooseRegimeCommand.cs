
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

    public override void Enact(ProcedureWriteKey key)
    {
        var player = key.Data.BaseDomain.PlayerAux.ByGuid[CommandingPlayerGuid];
        player.SetRegime(Regime.Entity(key.Data), key);
    }

    public override bool Valid(Data data)
    {
        return Regime.Entity(data).IsPlayerRegime(data) == false;
    }
}
