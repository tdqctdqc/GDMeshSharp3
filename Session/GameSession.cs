using Godot;
using System;
using System.Diagnostics;
using System.Linq;

public partial class GameSession : Node, ISession
{
    public Data Data { get; private set; }
    public Client Client { get; private set; }
    private ILogic _logic;
    public IServer Server { get; private set; }
    
    public static GameSession StartAsGenerator()
    {
        var s = new GameSession();
        var worldGen = s.SetAsGenerator();
        return s;
    }
    public static GameSession StartAsRemote()
    {
        var s = new GameSession();
        s.SetAsRemote();
        return s;
    }
    public static GameSession StartAsLoad(Data data)
    {
        var s = new GameSession();
        s.LoadAsHost(data);
        return s;
    }
    private GameSession()
    {
    }
    private void SetupPlayer(CreateWriteKey key)
    {
        Data.ClientPlayerData.SetLocalPlayerGuid(new Guid());
        Player.Create(Data.ClientPlayerData.LocalPlayerGuid, "Doot", key);
    }
    public override void _Process(double deltaD)
    {
        var delta = (float) deltaD;
        _logic?.Process(delta);
        Client?.Process(delta);
    }

    public WorldGenLogic SetAsGenerator()
    {
        var server = new DummyServer();
        StartServer(server);
        Data = new GenData();
        StartClient();
        
        var worldGen = new WorldGenLogic(this);
        _logic = worldGen;
        worldGen.FinishedGenSuccessfully = () => Client.SetupForGameData(Data);
        Client.SetupForGenerator(worldGen);
        return worldGen;
    }
    public void ResetAsGenerator(WorldGenLogic worldGen)
    {
        ((Node)Server)?.QueueFree();
        Server = null;
        Data = new GenData();
        StartClient();
        Client.SetupForGenerator(worldGen);
    }
    
    public void StartClient()
    {
        Client?.QueueFree();
        Client = new Client(this);
        AddChild(Client);
    }
    private void LoadAsHost(Data data)
    {
        Data = data;
        var hServer = new HostServer();
        var logic = new HostLogic(Data);
        _logic = logic;
        hServer.Setup(logic, Data, this);
        logic.SetDependencies(hServer, this, Data);
        StartServer(hServer);
        // SetupPlayer(new HostWriteKey(hServer, logic, Data, this));
        StartClient();
        // Client.SetupForGameplay(true, Data);
        Client.SetupForGameData(Data);
    }
    public void GeneratorToGameTransition()
    {
        var hServer = new HostServer();
        var logic = new HostLogic(Data);
        _logic = logic;
        hServer.Setup(logic, Data, this);
        logic.SetDependencies(hServer, this, Data);
        StartServer(hServer);
        logic.FirstTurn();
        SetupPlayer(new HostWriteKey(hServer, logic, Data, this));
        Client.SetupForGameplay(true, Data);
    }
    
    public void SetAsRemote()
    {
        Data = new Data();
        var logic = new RemoteLogic(Data, this);
        _logic = logic;
        var server = new RemoteServer();
        server.Setup(this, logic, Data);
        StartServer(server);
        StartClient();
        Client.SetupForGameplay(false, Data);
    }

    private void StartServer(IServer server)
    {
        Server = server;
        ((Node)server).Name = "Server";
        AddChild((Node)server);
    }

    // public override void _UnhandledInput(InputEvent e)
    // {
    //     var delta = (float)GetProcessDeltaTime();
    //     Client?.HandleInput(e, delta);
    // }
}
