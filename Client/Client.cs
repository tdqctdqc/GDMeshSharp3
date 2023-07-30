using System;
using System.Collections.Generic;
using System.Linq;
using Godot;

public partial class Client : Node, IClient
{
    public ClientWriteKey Key { get; private set; }
    public ClientSettings Settings { get; private set; }
    public UiRequests UiRequests { get; private set; }
    public CanvasLayer UiLayer { get; private set; }
    public Node2D GraphicsLayer { get; private set; }
    public Dictionary<Type, IClientComponent> Components { get; private set; }
    private GameSession _session;
    public Client(GameSession session)
    {
        _session = session;
        Setup();
    }
    private void Setup()
    {
        Key = new ClientWriteKey(_session.Data, _session);
        GraphicsLayer = new Node2D();
        AddChild(GraphicsLayer);
        UiLayer = new CanvasLayer();
        AddChild(UiLayer);
        
        _session = _session;
        Components = new Dictionary<Type, IClientComponent>();
        UiRequests = new UiRequests();
        Settings = ClientSettings.Load();
        AddComponent(new UiFrame(this));
        
        var cam = WorldCameraController.Construct(_session.Data);
        AddComponent(cam);
        AddChild(cam);
        
        AddComponent(new WindowManager(this));
        AddComponent(new PromptManager(this, _session.Data));
        AddComponent(new ClientTopBar(this, _session));
        
        AddComponent(new TooltipManager(_session.Data, this));
    }

    public override void _Process(double delta)
    {
        var values = Components.Values.ToList();
        foreach (var component in values)
        {
            component.Process((float)delta);
        }
    }

    public void AddComponent(IClientComponent component)
    {
        Components.Add(component.GetType(), component);
    }

    private void AddComponent(Type type, IClientComponent component)
    {
        Components.Add(type, component);
    }
    public bool HasComponent<T>() where T : IClientComponent
    {
        return Components.ContainsKey(typeof(T));
    }
    public void RemoveComponent<T>() where T : IClientComponent
    {
        RemoveComponent(typeof(T));
    }

    private void RemoveComponent(Type type)
    {
        if (Components.ContainsKey(type) == false) return;
        var c = Components[type];
        c.Disconnect?.Invoke();
        c.Node.QueueFree();
    }
    public T GetComponent<T>() where T : class, IClientComponent
    {
        if (Components.ContainsKey(typeof(T)) == false) return null;
        return (T)Components[typeof(T)];
    }
    public void HandleInput(InputEvent e, float delta)
    {
    }

    public void HandleCommand(Command c)
    {
        _session.Server.QueueCommandLocal(c);
    }

    void IClient.HandleInput(InputEvent e, float delta)
    {
        HandleInput(e, delta);
    }

    public void Process(float delta)
    {
        foreach (var c in Components.Values)
        {
            c.Process(delta);
        }
    }

    public void SetupForGenerator(WorldGenLogic wrapper)
    {
        var genUi = GeneratorUi.Construct(this, _session, wrapper);
        AddComponent(genUi);
    }
    public void SetupForGameplay(bool host, Data data)
    {
        RemoveComponent<GeneratorUi>();
        var gameUi = new GameplayUi(this, data, host);
        AddComponent(gameUi);
    }

    public void SetupForGameData(Data data)
    {
        var mapGraphics = new MapGraphics(data, this);
        AddComponent(mapGraphics);
        
        var mapGraphicsOptions = new MapGraphicsOptions(this);
        AddComponent(mapGraphicsOptions);
        
        GetComponent<WindowManager>().AddWindow(new RegimeOverviewWindow());
    }
}

public static class ClientExt
{
    public static ICameraController Cam(this Client c)
    {
        return c.GetComponent<WorldCameraController>();
    }
}