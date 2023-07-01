using System;
using System.Collections.Generic;
using System.Linq;
using Godot;

public partial class StartScene : Node
{
    private ButtonToken _genBtn, _remoteBtn, _sandbox;
    public override void _Ready()
    {
        _genBtn = ButtonToken.FindButtonCreateToken(this, "Generate", StartGenerator); 
        _remoteBtn = ButtonToken.FindButtonCreateToken(this, "Remote", StartAsClient); 
        _sandbox = ButtonToken.FindButtonCreateToken(this, "Sandbox", StartSandbox); 
    }

    private void StartGenerator()
    {
        Game.I.StartGeneratorSession();
        QueueFree();
    }

    private void StartAsClient()
    {
        Game.I.StartClientSession();
        QueueFree();
    }

    private void StartSandbox()
    {
        Game.I.StartSandbox();
        QueueFree();
    }
}