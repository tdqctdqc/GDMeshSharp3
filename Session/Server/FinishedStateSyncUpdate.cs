using System;
using System.Collections.Generic;
using System.Linq;
using Godot;

public class FinishedStateSyncUpdate : Update
{
    public Guid PlayerGuid { get; private set; }
    public FinishedStateSyncUpdate(Guid playerGuid) : base()
    {
        PlayerGuid = playerGuid;
    }

    public override void Enact(ProcedureKey key)
    {
        GD.Print("Finished state sync");
        key.Data.ClientPlayerData.SetLocalPlayerGuid(PlayerGuid);
        key.Data.Context.Calculate(key.Data);
        key.Data.Notices.InvokeFinishedStateSync(key.Data);
    }
}