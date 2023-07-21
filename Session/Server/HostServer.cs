using Godot;
using System;
using System.Collections.Generic;
using System.Linq;

public partial class HostServer : Node, IServer
{
    private HostWriteKey _key;
    private HostLogic _logic;
    private List<HostSyncer> _peers;
    private Dictionary<Guid, HostSyncer> _peersByGuid;
    private TcpServer _tcp;
    private int _port = 3306;
    public override void _Ready()
    {
        _peers = new List<HostSyncer>();
        _peersByGuid = new Dictionary<Guid, HostSyncer>();
        _tcp = new TcpServer();
        _tcp.Listen((ushort)_port);
    }

    public override void _Process(double delta)
    {
        if (_tcp.IsConnectionAvailable())
        {
            GD.Print("connection available");
            var peer = _tcp.TakeConnection();
            HandleNewPeer(peer);
        }
    }

    private void HandleNewPeer(StreamPeerTcp peer)
    {
        var packet = new PacketPeerStream();
        packet.StreamPeer = peer;
        var newPlayerGuid = Guid.NewGuid();
        var syncer = new HostSyncer(packet, _logic, newPlayerGuid);
        GD.Print("started syncing");
        syncer.Sync(newPlayerGuid, _key);
        GD.Print("Done syncing");
        _peers.Add(syncer);
        _peersByGuid.Add(newPlayerGuid, syncer);
    }
    public void SetDependencies(HostLogic logic, Data data, GameSession session)
    {
        _logic = logic;
        _key = new HostWriteKey(this, logic, data, session);
    }

    public void QueueMessage(Message m)
    {
        var bytes = m.Serialize();
        for (var i = 0; i < _peers.Count; i++)
        {
            _peers[i].QueuePacket(bytes);
        }
    }
    public void ReceiveLogicResult(LogicResults results, HostWriteKey key)
    {
        for (var i = 0; i < results.Messages.Count; i++)
        {
            var bytes = results.Messages[i].Serialize();
            for (var j = 0; j < _peers.Count; j++)
            {
                _peers[j].QueuePacket(bytes);
            }
        }
    }

    public void PushPackets(HostWriteKey key)
    {
        _peers.ForEach(p => p.PushPackets(key));
    }
    public void QueueCommandLocal(Command c)
    {
        _logic.CommandQueue.Enqueue(c);
    }
}
