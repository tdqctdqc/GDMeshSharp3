using Godot;
using System;
using System.Collections.Generic;
using System.Linq;

public partial class HostServer : Node, IServer
{
    private Data _data;
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
    public void Setup(HostLogic logic, Data data)
    {
        _logic = logic;
        _data = data;
    }
    public override void _Process(double delta)
    {
        if (_tcp.IsConnectionAvailable())
        {
            GD.Print("connection available");
            var peer = _tcp.TakeConnection();
            var newPlayer = _logic.MakeNewPlayer();
            HandleNewPeer(newPlayer, peer);
        }
    }

    private void HandleNewPeer(Player newPlayer,
        StreamPeerTcp peer)
    {
        var packet = new PacketPeerStream();
        packet.StreamPeer = peer;
        var newPlayerGuid = Guid.NewGuid();
        var syncer = new HostSyncer(packet, _logic, 
            newPlayerGuid);
        GD.Print("started syncing");
        syncer.Sync(newPlayer, _data);
        GD.Print("Done syncing");
        _peers.Add(syncer);
        _peersByGuid.Add(newPlayerGuid, syncer);
    }
    

    public void QueueMessage(Message m)
    {
        var bytes = m.Serialize(_data);
        for (var i = 0; i < _peers.Count; i++)
        {
            _peers[i].QueuePacket(bytes);
        }
    }

    public void SendMessageToClient(Procedure p, Guid clientGuid)
    {
        var bytes = p.Serialize(_data);
        _peersByGuid[clientGuid].QueuePacket(bytes);
    }
    public void ReceiveMessage(Message m)
    {
        var bytes = m.Serialize(_data);
        for (var j = 0; j < _peers.Count; j++)
        {
            _peers[j].QueuePacket(bytes);
        }
    }
    public void PushPackets()
    {
        _peers.ForEach(p => p.PushPackets());
    }
    public void QueueCommandLocal(Command c)
    {
        _logic.CommandQueue.Enqueue(c);
    }
}
