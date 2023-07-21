using System;
using System.Collections.Generic;
using System.Linq;
using Godot;

public partial class MainMenuClient : Node, IClient
{
    public TooltipManager TooltipManager { get; }
    public ClientWriteKey WriteKey { get; private set; }
    public UiRequests UiRequests { get; }
    public ICameraController Cam { get; }
    public ClientSettings Settings { get; }
    public UiRequests Requests { get; private set; }

    public MainMenuClient()
    {
        UiRequests = new UiRequests();
        var startScene = SceneManager.Instance<StartScene>();
        AddChild(startScene);
        WriteKey = new ClientWriteKey(null, null);
        Settings = ClientSettings.Load();
        Requests = new UiRequests();
    }
    public void HandleInput(InputEvent e, float delta)
    {
    }

    public void Process(float delta)
    {
    }
    
}
