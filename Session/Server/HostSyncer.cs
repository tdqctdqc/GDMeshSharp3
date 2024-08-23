using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Godot;

public class HostSyncer : Syncer
{
    private Queue<byte[]> _peerQueue;
    public HostSyncer(PacketPeerStream packetStream, 
        HostLogic logic, Guid clientGuid) 
        : base(packetStream, 
            m =>
            {
                if (m is Command c)
                {
                    logic.CommandQueue.Enqueue(c);
                }
                else throw new Exception();
            }, logic.PKey.Data)
    {
        _peerQueue = new Queue<byte[]>();
    }

    public void Sync(Player newPlayer, Data data)
    {
        foreach (var e in data.EntitiesById.Values)
        {
            var u = EntityCreationUpdate.Create(e, data);
            QueuePacket(u.Serialize(data));
        }
        
        var done = new FinishedStateSyncUpdate(newPlayer.PlayerGuid);
        var bytes = done.Serialize(data);
        QueuePacket(bytes);
        PushPackets();
    }
    public void QueuePacket(byte[] packet)
    {
        _peerQueue.Enqueue(packet);
    }
    public void PushPackets()
    {
        bool push = true;
        var count = _peerQueue.Count;
        for (var i = 0; i < count; i++)
        {
            PushPacket(_peerQueue.Dequeue());
        }
    }
}