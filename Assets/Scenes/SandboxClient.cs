using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Godot;

public partial class SandboxClient : Node, IClient
{
    public ClientWriteKey Key { get; private set; }
    public ICameraController Cam { get; private set; }
    private Label _degrees, _mousePos;
    private Dictionary<int, Node2D> _triGraphics;
    private Node2D _mouseOverTriGraphics;
    private Line2D _mouseLine;
    private CanvasLayer _canvas;
    private List<int> _sectionTriIndices;
    private RegionDebugGraphic _regionTest;
    private Control _debugHook;
    public ClientSettings Settings { get; private set; }
    public ClientRequests Requests { get; private set; }

    public SandboxClient()
    {
        Settings = ClientSettings.Load();
    }

    public override void _Ready()
    {
        this.AssignChildNode(ref _degrees, "Degrees");
        this.AssignChildNode(ref _mousePos, "MousePos");
        this.AssignChildNode(ref _debugHook, "DebugHook");
        _regionTest = new RegionDebugGraphic();
        AddChild(_regionTest);
        ExceptionCatcher.Try(() => _regionTest.Setup(DisplayError, _debugHook), DisplayError);
    }

    private void DisplayError(DisplayableException e)
    {
        _regionTest.QueueFree();
        AddChild(e.GetGraphic());
    }
    public void Setup(Vector2 home, SandboxSession session)
    {
        Requests = new ClientRequests(session);

        Key = new ClientWriteKey(null, null);
        this.AssignChildNode(ref _canvas, "Canvas");
        var cam = WorldCameraController.Construct(null); //todo make dummy
        AddChild(cam);
        // TreeEntered += cam.MakeCurrent;
        Cam = cam;

        _mouseOverTriGraphics = new Node2D();
        _mouseLine = new Line2D();
        _mouseLine.DefaultColor = Colors.Red;
        _mouseLine.Width = 10;
        AddChild(_mouseLine);
    }


    public void HandleInput(InputEvent e, float delta)
    {
        if (e is InputEventMouseMotion m)
        {
            
            
        }
    }

    public void DrawTri(Triangle tri)
    {
        var mb = new MeshBuilder();
        mb.AddTri(tri, ColorsExt.GetRandomColor());
        var mi = mb.GetMeshInstance();
        _triGraphics.Add(_triGraphics.Count, mi);
        AddChild(mi);
    }
    
    public void Process(float delta, bool gameStateChanged)
    {
        
    }

}