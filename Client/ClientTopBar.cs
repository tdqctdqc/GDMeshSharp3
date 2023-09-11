using System;
using System.Collections.Generic;
using System.Linq;
using Godot;

public partial class ClientTopBar : HBoxContainer, IClientComponent
{
    public Action Disconnect { get; set; }

    public ClientTopBar(Client client, GameSession session)
    {
        var key = new WriteKey(session.Data, session);
        this.AddWindowButton<LoggerWindow>("Logger");
        this.AddWindowButton<EntityOverviewWindow>("Entities");
        this.AddWindowButton<ClientSettingsWindow>("Settings");
        this.AddButton("Test Serialization", () => session.Data.Serializer.Test(session.Data));
        this.AddButton("Save", () => Saver.Save(session.Data, key));
        this.AddButton("Load", () => Saver.Load());
        this.AddButton("Test Nav", () => Saver.TestNav(session.Data));
        this.AddIntButton("Jump to Poly", i =>
        {
            var poly = session.Data.Get<MapPolygon>(i);
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
