using System;
using System.Collections.Generic;
using System.Linq;
using Godot;

public class FinishedStateSyncUpdate : Update
{
    public Guid PlayerGuid { get; private set; }
    public static FinishedStateSyncUpdate Create(Guid playerGuid, HostWriteKey key)
    {
        return new FinishedStateSyncUpdate(playerGuid);
    }
    public FinishedStateSyncUpdate(Guid playerGuid) : base()
    {
        PlayerGuid = playerGuid;
    }

    public override void Enact(ProcedureWriteKey key)
    {
        GD.Print("Finished state sync");
        key.Data.ClientPlayerData.SetLocalPlayerGuid(PlayerGuid);
        key.Data.Notices.FinishedStateSync?.Invoke();
    }
}