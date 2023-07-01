using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Godot;

public partial class SandboxSession : Node, ISession
{
    public RefFulfiller RefFulfiller => null;
    private Vector2 _home;
    IClient ISession.Client => Client;
    public IServer Server { get; private set; }
    public SandboxClient Client { get;  set; }
    public override void _Ready()
    {
        var client = SceneManager.Instance<SandboxClient>();
        AddChild(client);
        _home = Vector2.Zero;
        Client = client;
    }

    public void Setup()
    {
        Client.Setup(Vector2.Zero, this);
    }
    public override void _Process(double delta)
    {
        Client?.Process((float)delta, false);
    }

    public override void _UnhandledInput(InputEvent e)
    {
        Client?.HandleInput(e, (float)GetProcessDeltaTime());
    }
}