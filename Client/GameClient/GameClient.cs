using Godot;
using System;

public partial class GameClient : Node, IClient
{
    private EntityOverviewWindow _entityOverviewWindow;
    public GameUi Ui { get; private set; }
    private IServer _server;
    public ICameraController Cam { get; private set; }
    public MapGraphics MapGraphics { get; private set; }
    public ClientWriteKey WriteKey { get; private set; }
    public UiRequests UiRequests { get; private set; }
    public ClientSettings Settings { get; private set; }

    public GameClient()
    {
        UiRequests = new UiRequests();
    }
    public override void _Ready()
    {
        
    }

    public void Process(float delta)
    {
        if (GetParent() == null) return;
        MapGraphics?.Process(delta);
        Ui?.Process(delta, Cam, WriteKey);
    }
    public void Setup(GameSession session, IServer server)
    {
        Settings = ClientSettings.Load();
        WriteKey = new ClientWriteKey(session.Data, session);
        var cam = WorldCameraController.Construct(session.Data);
        AddChild(cam);
        Cam = cam;

        MapGraphics = new MapGraphics(WriteKey);
        AddChild(MapGraphics);
        
        BuildUi(session.Data, WriteKey.Session.Server);
    }

    private void BuildUi(Data data, IServer server)
    {
        Ui = GameUi.Construct(this, server is HostServer, data, MapGraphics);
        AddChild(Ui);
        _server = server;
        _entityOverviewWindow = EntityOverviewWindow.Get(data);
        AddChild(_entityOverviewWindow);
        Ui.MapGraphicsOptions.Setup(MapGraphics);
    }
    public void HandleInput(InputEvent e, float delta)
    {
        
    }
}
