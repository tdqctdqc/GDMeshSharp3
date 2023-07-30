using System;
using System.Collections.Generic;
using System.Linq;
using Godot;

public partial class StartScene : Node
{
    private Button _genBtn, _remoteBtn, _sandbox;
    public override void _Ready()
    {
        var container = GetNode<Container>("Container");
        _genBtn = container.GetNode<Button>("Generate");
        _genBtn.ButtonUp += StartAsHost;

        _remoteBtn = container.GetNode<Button>("Remote");
        _remoteBtn.ButtonUp += StartAsClient;

        _sandbox = container.GetNode<Button>("Sandbox");
        _sandbox.ButtonUp += StartSandbox;
    }

    private void StartAsHost()
    {
        GD.Print("starting as host");
        Game.I.StartHostSession();
        QueueFree();
    }

    private void StartAsClient()
    {
        Game.I.StartClientSession();
        QueueFree();
    }

    private void StartSandbox()
    {
        QueueFree();
    }
}