using Godot;
using System;

public partial class GameClient : Node, IClient
{
    private EntityOverviewWindow _entityOverviewWindow;
    public GameUi Ui { get; private set; }
    private IServer _server;
    public ClientRequests Requests { get; private set; }
    public ICameraController Cam { get; private set; }
    public MapGraphics Graphics { get; private set; }
    public ClientWriteKey Key { get; private set; }
    public ClientSettings Settings { get; private set; }
    
    public override void _Ready()
    {
        
    }

    public void Process(float delta, bool gameStateChanged)
    {
        if (GetParent() == null) return;
        Graphics?.Process(delta);
        Ui?.Process(delta, Cam, Key);
        if (gameStateChanged)
        {
            Game.I.Logger.Log("updating graphics", LogType.Logic);
            Graphics.Update();
        }
    }
    public void Setup(GameSession session, IServer server, MapGraphics graphics)
    {
        Requests = new ClientRequests(session);
        Requests.GiveTree(session.Data.EntityTypeTree);
        Settings = ClientSettings.Load();
        Key = new ClientWriteKey(session.Data, session);
        var cam = WorldCameraController.Construct(session.Data);
        AddChild(cam);
        Cam = cam;

        if (graphics == null)
        {
            BuildGraphics(session.Data);
        }
        else
        {
            Graphics = graphics;
        }
        AddChild(Graphics);
        BuildUi(session.Data, Key.Session.Server);
    }
    
    private void BuildGraphics(Data data)
    {
        Graphics = new MapGraphics();
        Graphics.Setup(Key);
    }

    private void BuildUi(Data data, IServer server)
    {
        Ui = GameUi.Construct(this, server is HostServer, data, Graphics);
        AddChild(Ui);
        _server = server;
        _entityOverviewWindow = EntityOverviewWindow.Get(data);
        AddChild(_entityOverviewWindow);
    }
    public void HandleInput(InputEvent e, float delta)
    {
        
    }
}
