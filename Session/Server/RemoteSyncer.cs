using System;
using System.Collections.Generic;
using System.Linq;
using Godot;

public class RemoteSyncer : Syncer
{
    private Data _data;
    public RemoteSyncer(PacketPeerStream packetStream, 
        RemoteLogic logic) 
        : base(packetStream, logic.HandleMessage,
            logic.PKey.Data)
    {
        _data = logic.PKey.Data;
    }

    public void SendCommand(Command command)
    {
        var bytes = command.Serialize(_data);
        PushPacket(bytes);
    }
}