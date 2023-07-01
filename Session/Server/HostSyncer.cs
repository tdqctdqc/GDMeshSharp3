using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Godot;

public class HostSyncer : Syncer
{
    private Queue<byte[]> _peerQueue;
    public HostSyncer(PacketPeerStream packetStream, HostLogic logic, Guid fromGuid) 
        : base(packetStream, 
            m => {
                if (m is Command c)
                {
                    c.SetGuid(fromGuid);
                }
                m.HandleHost(logic);
            })
    {
        _peerQueue = new Queue<byte[]>();
    }

    public void Sync(Guid newPlayerGuid, HostWriteKey key)
    {
        GD.Print("Syncing");
        Player.Create(newPlayerGuid, "doot", key);

        var data = key.Data;
        foreach (var e in data.Entities.Values)
        {
            var u = EntityCreationUpdate.Create(e, key);
            QueuePacket(u.Wrap());
        }
        
        var done = FinishedStateSyncUpdate.Create(newPlayerGuid, key);
        var bytes = done.Wrap();
        QueuePacket(bytes);
        PushPackets(key);
    }
    public void QueuePacket(byte[] packet)
    {
        _peerQueue.Enqueue(packet);
    }
    public void PushPackets(HostWriteKey key)
    {
        bool push = true;
        var count = _peerQueue.Count;
        for (var i = 0; i < count; i++)
        {
            PushPacket(_peerQueue.Dequeue());
        }
    }
}