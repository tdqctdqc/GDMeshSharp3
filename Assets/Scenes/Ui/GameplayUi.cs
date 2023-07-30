using System;
using System.Collections.Generic;
using System.Linq;
using Godot;

public partial class GameplayUi : Node, IClientComponent
{
    Node IClientComponent.Node => this;
    public Action Disconnect { get; set; }

    public GameplayUi(Client client, Data data, bool host)
    {
        client.UiLayer.AddChild(this);
        var topBar = new GameUiTopBar(client, host, data);
        client.AddComponent(topBar);

        Disconnect += () =>
        {
            client.RemoveComponent<GameUiTopBar>();
        };
    }
    public void Process(float delta)
    {
        
    }
}
