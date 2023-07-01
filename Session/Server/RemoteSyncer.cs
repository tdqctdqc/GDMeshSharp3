using System;
using System.Collections.Generic;
using System.Linq;
using Godot;

public class RemoteSyncer : Syncer
{
    public RemoteSyncer(PacketPeerStream packetStream, RemoteLogic logic) 
        : base(packetStream, 
            m => m.HandleRemote(logic))
    {
    }

    public void SendCommand(Command command)
    {
        var bytes = command.Wrap();
        PushPacket(bytes);
    }
    
}