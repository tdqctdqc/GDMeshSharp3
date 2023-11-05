using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using Godot;

public partial class Client : Node, IClient
{
    public Data Data => _session.Data;
    public ClientWriteKey Key { get; private set; }
    public ClientSettings Settings { get; private set; }
    public UiRequests UiRequests { get; private set; }
    public ConcurrentQueue<Action> QueuedUpdates { get; }
    public Control UiLayer { get; private set; }
    public Node2D GraphicsLayer { get; private set; }
    public IServer Server => _session.Server;
    public ILogic Logic => _session.Logic;
    public RefAction UiTick { get; private set; }
    private TimerAction _uiTickTimer;
    private ISession _session;
    public Dictionary<Type, IClientComponent> Components { get; private set; }
    public Client(ISession session)
    {
        _session = session;
        QueuedUpdates = new ConcurrentQueue<Action>();
        UiTick = new RefAction();
        _uiTickTimer = new TimerAction(.1f, 0f, UiTick.Invoke);
        Setup();
    }
    private void Setup()
    {
        Key = new ClientWriteKey(Data);
        GraphicsLayer = new Node2D();
        AddChild(GraphicsLayer);
        var ui = new CanvasLayer();
        UiLayer = new Control();
        UiLayer.MouseFilter = Control.MouseFilterEnum.Pass;
        ui.AddChild(UiLayer);
        UiLayer.FocusMode = Control.FocusModeEnum.None;
        AddChild(ui);
        
        Components = new Dictionary<Type, IClientComponent>();
        UiRequests = new UiRequests();
        Settings = ClientSettings.Load();
        AddComponent(new UiFrame(this));
        
        var cam = WorldCameraController.Construct(Data);
        AddComponent(cam);
        AddChild(cam);
        
        AddComponent(new WindowManager(this));
        GetComponent<WindowManager>().AddWindow(ClientSettingsWindow.Get(Settings));
        
        
        AddComponent(new PromptManager(this));
        AddComponent(new ClientTopBar(this));
        AddComponent(new TooltipManager(Data, this));
    }
    public override void _Process(double delta)
    {
        _uiTickTimer.Process(delta);
        var values = Components.Values.ToList();
        foreach (var component in values)
        {
            component.Process((float)delta);
        }

        while (QueuedUpdates.TryDequeue(out var u))
        {
            u.Invoke();
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
        Server.QueueCommandLocal(c);
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
        var genUi = GeneratorUi.Construct(this, wrapper);
        AddComponent(genUi);
    }
    public void SetupForGameplay(bool host)
    {
        RemoveComponent<GeneratorUi>();
        var gameUi = new GameplayUi(this, Data, host);
        AddComponent(gameUi);
    }

    public void SetupForGameData()
    {
        var mapGraphics = new MapGraphics(this);
        AddComponent(mapGraphics);
        
        var mapGraphicsOptions = new MapGraphicsOptions(this);
        AddComponent(mapGraphicsOptions);
        
        GetComponent<WindowManager>().AddWindow(new RegimeOverviewWindow());
        GetComponent<WindowManager>().AddWindow(new MarketOverviewWindow(Data));
    }
}

public static class ClientExt
{
    public static ICameraController Cam(this Client c)
    {
        return c.GetComponent<WorldCameraController>();
    }
}