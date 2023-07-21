using System;
using System.Collections.Generic;
using System.Linq;
using Godot;

public class RemoteSyncer : Syncer
{
    public RemoteSyncer(PacketPeerStream packetStream, RemoteLogic logic) 
        : base(packetStream, 
            m => m.Enact(logic.PKey))
    {
    }

    public void SendCommand(Command command)
    {
        var bytes = command.Serialize();
        PushPacket(bytes);
    }
    
}