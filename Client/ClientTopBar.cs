using System;
using System.Collections.Generic;
using System.Linq;
using Godot;

public partial class ClientTopBar : HBoxContainer, IClientComponent
{
    public Action Disconnect { get; set; }

    public ClientTopBar(Client client)
    {
        this.AddButton("Logger",
            () => LoggerWindow.Open(client.Data));
        this.AddButton("Entities",
            () => EntityOverviewWindow.Open(client.Data));
        this.AddButton("Settings",
            () => ClientSettingsWindow.Open(client.Settings));
        this.AddButton("Save", () => Saver.Save(client.Data));
        this.AddButton("Load", () => Saver.Load());
        this.AddButton("Test", () => Serializer.TestCustom(client.Data));
        this.AddIntButton("Jump to Poly", i =>
        {
            var poly = client.Data.Get<MapPolygon>(i);
            if (poly == null) return;
            client.Cam().SetPos(poly.Center);
        });
        this.AddIntButton("Jump to Cell", i =>
        {
            var wp = PlanetDomainExt.GetPolyCell(i, client.Data);
            if (wp == null) return;
            client.Cam().SetPos(wp.GetCenter());
        });
        this.AddIntButton("Jump to Group", i =>
        {
            if (client.Data.EntitiesById.TryGetValue(i, out var e)
                && e is Army a)
            {
                client.Cam().SetPos(a.GetHomeCell(client.Data).GetCenter());
            }
        });
        
        var uiFrame = client.GetComponent<UiFrame>();
        uiFrame.AddTopBar(this);
    }
    public void Process(float delta)
    {
    }
    Node IClientComponent.Node => this;

}
