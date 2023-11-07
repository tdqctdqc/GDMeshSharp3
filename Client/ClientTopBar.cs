using System;
using System.Collections.Generic;
using System.Linq;
using Godot;

public partial class ClientTopBar : HBoxContainer, IClientComponent
{
    public Action Disconnect { get; set; }

    public ClientTopBar(Client client)
    {
        this.AddWindowButton<LoggerWindow>("Logger");
        this.AddWindowButton<EntityOverviewWindow>("Entities");
        this.AddWindowButton<ClientSettingsWindow>("Settings");
        this.AddButton("Save", () => Saver.Save(client.Data));
        this.AddButton("Load", () => Saver.Load());
        this.AddButton("Test", () => Serializer.TestCustom(client.Data));
        this.AddIntButton("Jump to Poly", i =>
        {
            var poly = client.Data.Get<MapPolygon>(i);
            if (poly == null) return;
            client.Cam().JumpTo(poly.Center);
        });
        var uiFrame = client.GetComponent<UiFrame>();
        uiFrame.AddTopBar(this);
    }
    public void Process(float delta)
    {
    }
    Node IClientComponent.Node => this;

}
