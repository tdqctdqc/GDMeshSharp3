using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;

public partial class RemoteServer : Node, IServer
{
    public int NetworkId { get; private set; }
    public Action ReceivedStateTransfer { get; set; }

    private ServerWriteKey _key;
    
    private ENetMultiplayerPeer _network;
    private StreamPeerTcp _streamPeer;
    private PacketPeerStream _packetStream;
    private RemoteSyncer _syncer;
    private string _ip = "127.0.0.1";
    private int _port = 3306;

    public void Setup(GameSession session, RemoteLogic logic, Data data)
    {
        _key = new ServerWriteKey(data, session);
        _streamPeer = new StreamPeerTcp();
        if (
            // _streamPeer.IsConnectedToHost() == false
            true
            )
        {
            _streamPeer.ConnectToHost(_ip, _port);
            _packetStream = new PacketPeerStream();
            _packetStream.StreamPeer = _streamPeer;
            _syncer = new RemoteSyncer(_packetStream, logic);
            SetProcess(true);
        }
    }
    public override void _Ready()
    {
        // _network = new ENetMultiplayerPeer();
        // _network.CreateClient(_ip, _port);
        //
        // GetTree().NetworkPeer = _network;
        // _network.Connect("connection_failed", this, nameof(OnConnectionFailed));
    }
    public void OnConnectionSucceeded()
    {
        // NetworkId = _network.GetUniqueId();
        // GD.Print("connection succeeded, id is " + NetworkId);
    }

    public void OnConnectionFailed()
    {
        GD.Print("connection failed");
    }

    public void QueueCommandLocal(Command c)
    {
        _syncer.SendCommand(c);
    }
}
