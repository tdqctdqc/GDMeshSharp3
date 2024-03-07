
using System;

public class SetPlayerRegimeProcedure : Procedure
{
    public ERef<Regime> Regime { get; private set; }
    public Guid PlayerGuid { get; private set; }

    public SetPlayerRegimeProcedure(ERef<Regime> regime, Guid playerGuid)
    {
        Regime = regime;
        PlayerGuid = playerGuid;
    }

    public override void Enact(ProcedureWriteKey key)
    {
        var player = key.Data.BaseDomain.PlayerAux.ByGuid[PlayerGuid];
        player.SetRegime(Regime.Entity(key.Data), key);
    }

    public override bool Valid(Data data)
    {
        return Regime.Entity(data).IsPlayerRegime(data) == false;
    }
}