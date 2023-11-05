using Godot;
using System;
using System.Diagnostics;
using System.Linq;

public partial class GameSession : Node, ISession
{
    public Data Data { get; private set; }
    public Client Client { get; private set; }
    public ILogic Logic { get; private set; }
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
    public override void _Process(double deltaD)
    {
        var delta = (float) deltaD;
        Logic?.Process(delta);
        Client?.Process(delta);
    }

    public WorldGenLogic SetAsGenerator()
    {
        var worldGen = new WorldGenLogic(this);
        Logic = worldGen;
        
        var server = new HostServer();
        StartServer(server);
        Data = new GenData();
        StartClient();
        
        
        worldGen.FinishedGenSuccessfully = () => Client.SetupForGameData();
        worldGen.FinalizeGen += GeneratorToGameTransition;
        Client.SetupForGenerator(worldGen);
        return worldGen;
    }
    public void ResetAsGenerator(WorldGenLogic worldGen)
    {
        //todo reset logic as well
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
        Logic = logic;
        hServer.Setup(logic, Data, this);
        logic.SetDependencies(hServer, this, Data);
        StartServer(hServer);
        StartClient();
        Client.SetupForGameData();
    }
    public void GeneratorToGameTransition()
    {
        Data.Notices.ExitedGen.Invoke();
        var hServer = new HostServer();
        var logic = new HostLogic(Data);
        Logic = logic;
        hServer.Setup(logic, Data, this);
        logic.SetDependencies(hServer, this, Data);
        StartServer(hServer);
        
        logic.Start();
        Client.SetupForGameplay(true);
    }
    
    public void SetAsRemote()
    {
        Data = new Data();
        var logic = new RemoteLogic(Data, this);
        Logic = logic;
        var server = new RemoteServer();
        server.Setup(this, logic, Data);
        StartServer(server);
        StartClient();
        Client.SetupForGameplay(false);
    }

    private void StartServer(IServer server)
    {
        Server = server;
        ((Node)server).Name = "Server";
        AddChild((Node)server);
    }
}
