using Godot;
using System;
using System.Diagnostics;
using System.Linq;

public partial class GameSession : Node, IDataSession
{
    public Data Data { get; private set; }
    public IClient Client => _client;
    private GameClient _client;
    private ILogic _logic;
    public IServer Server { get; private set; }
    private WriteKey _key;
    public override void _Process(double deltaD)
    {
        var delta = (float) deltaD;
        _logic?.Process(delta);
        Client?.Process(delta);

    }

    public void Setup()
    {
        
    }
    public void StartAsHost(Data data)
    {
        Data = data;
        Data.ClientPlayerData.SetLocalPlayerGuid(new Guid());
        var hServer = new HostServer();
        Server = hServer;
        var logic = new HostLogic(Data);
        _logic = logic;
        var hKey = new HostWriteKey(hServer, logic, data, this);
        _key = hKey;
        
        hServer.SetDependencies(logic, Data, this);
        logic.SetDependencies(hServer, this, Data);
        StartServer(hServer);
        Player.Create(_key.Data.ClientPlayerData.LocalPlayerGuid, "Doot", hKey);
        StartClient(hServer);
        logic.Start();
        _client.Ui.Prompts.AddPromptIcon(Prompt.GetChooseRegimePrompt(hKey));
    }
    
    public void StartAsRemote()
    {
        Data = new Data();

        var logic = new RemoteLogic(Data, this);
        _logic = logic;
        var server = new RemoteServer();
        Server = server;
        _key = new WriteKey(Data, this);

        Data.Notices.FinishedStateSync.Subscribe(() =>
        {
            StartClient(server);
        });
        server.Setup(this, logic, Data);
        StartServer(server);
    }

    private void StartServer(IServer server)
    {
        ((Node)server).Name = "Server";
        AddChild((Node)server);
    }

    private void StartClient(IServer server)
    {
        _client = new GameClient();
        _client.Setup(this, server);
        AddChild((Node)Client);
    }
    public override void _UnhandledInput(InputEvent e)
    {
        var delta = (float)GetProcessDeltaTime();
        Client?.HandleInput(e, delta);
    }
}
