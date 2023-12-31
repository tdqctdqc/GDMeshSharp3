
using System;
using Godot;

public partial class UiController : Node, IClientComponent
{
    private Client _client;
    public UiMode Mode { get; private set; }
    public Node Node => this;

    public UiController(Client client)
    {
        _client = client;
        Disconnect += () => throw new Exception();
        Mode = new BlankMode(client);
    }

    public void SetMode(UiMode mode)
    {
        Mode = mode;
    }

    public Action Disconnect { get; set; }
    public void Process(float delta)
    {
        Mode?.Process(delta);
    }
}