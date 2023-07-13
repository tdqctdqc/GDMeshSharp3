using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Godot;

public partial class GeneratorClient : Node, IClient
{
    public ICameraController Cam { get; private set; }
    public ClientWriteKey WriteKey { get; private set; }
    public MapGraphics MapGraphics { get; private set; }
    public CanvasLayer CanvasLayer => _ui;
    private GeneratorUi _ui; 
    public ClientSettings Settings { get; private set; }
    public ClientRequests Requests { get; private set; }
    private Node2D _camTest;
    public override void _Ready()
    {
        
    }

    public void Setup(GeneratorSession session)
    {
        this.ClearChildren();
        Requests = new ClientRequests(session);
        Requests.GiveTree(session.Data.EntityTypeTree);
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
        _ui?.Process(delta, Cam);
        MapGraphics?.Process(delta);
    }

    public void StartMapGraphics()
    {
        MapGraphics = new MapGraphics();
        MapGraphics.Setup(WriteKey);
        AddChild(MapGraphics);
    }
    public override void _ExitTree()
    {
        GD.Print("freeing gen client");
    }
}