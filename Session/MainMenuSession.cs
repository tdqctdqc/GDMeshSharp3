using System;
using System.Collections.Generic;
using System.Linq;using Godot;

public partial class MainMenuSession : Node
{
    public Client Client { get; }
    public IServer Server { get; }

    public MainMenuSession()
    {
        Server = new DummyServer();
        var client = new MainMenuClient();
        AddChild(client);
    }

    public void Setup()
    {
    }
}
