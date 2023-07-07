
using System;
using Godot;

public class ChooseRegimeCommand : Command
{
    public EntityRef<Regime> Regime { get; private set; }
    public ChooseRegimeCommand(EntityRef<Regime> regime) : base()
    {
        Regime = regime;
    }

    public override void Enact(HostWriteKey key, Action<Procedure> queueProcedure)
    {
        var player = key.Data.BaseDomain.PlayerAux.ByGuid[CommandingPlayerGuid];
        player.Set<EntityRef<Regime>>(nameof(player.Regime), Regime, key);
    }

    public override bool Valid(Data data)
    {
        return Regime.Entity(data).IsPlayerRegime(data) == false;
    }
}
