using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Godot;

public partial class GeneratorClient : Node, IClient
{
    public ICameraController Cam { get; private set; }
    public ClientWriteKey WriteKey { get; private set; }
    public UiRequests UiRequests { get; private set; }
    public MapGraphics MapGraphics { get; private set; }
    public CanvasLayer CanvasLayer => _ui;
    private GeneratorUi _ui; 
    public ClientSettings Settings { get; private set; }
    public LogicRequests Requests { get; private set; }
    private Task<MapGraphics> _setupMapGraphics;
    private CancellationTokenSource _setupMapGraphicsToken;
    private Node2D _camTest;

    public GeneratorClient()
    {
        UiRequests = new UiRequests();
    }
    public override void _Ready()
    {
        
    }

    public void Setup(GeneratorSession session)
    {
        this.ClearChildren();
        Requests = new LogicRequests();
        WriteKey = new ClientWriteKey(session.Data, session);
        Settings = ClientSettings.Load();
        
        var cam = WorldCameraController.Construct(session.Data);
        AddChild(cam);
        Cam = cam;
        
        _ui = GeneratorUi.Construct(this, session);
        AddChild(_ui);
    }
    public void HandleInput(InputEvent e, float delta)
    {
    }
    public void Process(float delta)
    {
        if (_setupMapGraphics != null && _setupMapGraphics.IsCompleted)
        {
            MapGraphics = _setupMapGraphics.Result;
            AddChild(MapGraphics);
            _ui.MapGraphicsOptions.Setup(MapGraphics);
            _setupMapGraphics = null;
        }
        _ui?.Process(delta, Cam);
        MapGraphics?.Process(delta);
    }

    public void StartMapGraphics()
    {
        _setupMapGraphicsToken = new CancellationTokenSource();
        _setupMapGraphics = Task.Run(() =>
        {
            var mapGraphics = new MapGraphics();
            mapGraphics.Setup(WriteKey);
            return mapGraphics;
        }, _setupMapGraphicsToken.Token);
    }
    public override void _ExitTree()
    {
        _setupMapGraphicsToken?.Cancel();
    }
}