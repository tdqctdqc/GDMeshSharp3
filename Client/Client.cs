using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Godot;

public partial class Client : Node, IClient
{
    public Data Data => Session.Data;
    public ClientCallbacks Callbacks { get; private set; }
    public ClientNotices Notices { get; private set; }
    public ClientSettings Settings { get; private set; }
    public UiController UiController { get; private set; }
    public ConcurrentQueue<Action> QueuedUpdates { get; }
    public CanvasLayer UiCanvas { get; private set; }
    public Control UiLayer { get; private set; }
    public Node2D GraphicsLayer { get; private set; }
    public IServer Server => Session.Server;
    public ILogic Logic => Session.Logic;
    public RefAction UiTick { get; private set; }
    private TimerAction _uiTickTimer;
    public ISession Session { get; private set; }
    public WindowHolder WindowHolder => GetComponent<WindowHolder>();
    public Dictionary<Type, IClientComponent> Components { get; private set; }
    
    public Client(ISession session)
    {
        Notices = new ClientNotices();
        Session = session;
        QueuedUpdates = new ConcurrentQueue<Action>();
        UiTick = new RefAction();
        _uiTickTimer = new TimerAction(.1f, 0f, UiTick.Invoke);
        Callbacks = new ClientCallbacks();
        Setup();
    }
    private void Setup()
    {
        GraphicsLayer = new Node2D();
        AddChild(GraphicsLayer);
        UiCanvas = new CanvasLayer();
        UiLayer = new Control();
        UiLayer.MouseFilter = Control.MouseFilterEnum.Stop;
        UiCanvas.AddChild(UiLayer);
        UiLayer.FocusMode = Control.FocusModeEnum.None;
        
        AddChild(UiCanvas);
        
        Components = new Dictionary<Type, IClientComponent>();
        Settings = ClientSettings.Load();
        AddComponent(new UiFrame(this));
        
        var cam = WorldCameraController.Construct(Data);
        AddComponent(cam);
        AddChild(cam);
        AddComponent(new WindowHolder(this));
        AddComponent(new PromptManager(this));
        AddComponent(new ClientTopBar(this));
        AddComponent(new TooltipManager(Data, this));
        UiController = new UiController(this); 
        AddComponent(UiController);
    }
    public override void _Process(double delta)
    {
        _uiTickTimer.Process(delta);
        var values = Components.Values.ToList();

        lock (Session.Logic.Lock)
        {
            foreach (var component in values)
            {
                component.Process((float)delta);
            }
            while (QueuedUpdates.TryDequeue(out var u))
            {
                u.Invoke();
            }
        }
        
    }
    public override void _UnhandledInput(InputEvent e)
    {
        this.Cam()?.HandleInput(e);
        
        if (GetComponent<MapGraphics>() is MapGraphics mg
            && mg.UiElements.HandleInput(e,
                this.Cam().GetMousePosInMapSpace(),
                this))
        {
            return;
        }
        UiController.Mode.HandleInput(e);
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
        
        var uiFrame = GetComponent<UiFrame>();
        
        UiController.ModeOption.SettingChanged.SubscribeForNode(v =>
        {
            var newMode = v.newVal;
            if (newMode is not null)
            {
                uiFrame.LeftBar.ShowPanel(newMode.GetControl(this));
            }
            else
            {
                uiFrame.LeftBar.HidePanel();
            }
        }, uiFrame.LeftBar);
        
        uiFrame.LeftBar.Add(() => uiFrame.LeftBar.ShowPanel(new MapGraphicsOptionsPanel(this)),
            "Map Graphics Options");
        
        uiFrame.LeftBar.Add(() => UiController.ModeOption.Choose<ConstructionMode>(),
            "Construct");
        
        uiFrame.LeftBar.Add(() => UiController.ModeOption.Choose<ArmyMode>(),
            "Armies");
        
        uiFrame.LeftBar.Add(() => UiController.ModeOption.Choose<PolyMode>(),
            "Poly");
        
        uiFrame.LeftBar.Add(() => UiController.ModeOption.Choose<PathFindMode>(),
            "Pathfind");
        
        uiFrame.LeftBar.Add(() => UiController.ModeOption.Choose<MilPlanningMode>(),
            "Military Planning");
        // UiController.ModeOption.Choose<BlankMode>();
    }
}

public static class ClientExt
{
    public static ICameraController Cam(this Client c)
    {
        return c.GetComponent<WorldCameraController>();
    }
}