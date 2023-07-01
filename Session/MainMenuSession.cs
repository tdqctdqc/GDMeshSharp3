using System;
using System.Collections.Generic;
using System.Linq;using Godot;

public partial class MainMenuSession : Node, ISession
{
    public RefFulfiller RefFulfiller { get; }
    public IClient Client { get; }
    public IServer Server { get; }

    public MainMenuSession()
    {
        RefFulfiller = new RefFulfiller(null);
        Server = new DummyServer();
        var client = new MainMenuClient();
        AddChild(client);
        Client = client;
    }

    public void Setup()
    {
    }
}
