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
        this.AddIntButton("Jump to Cell", i =>
        {
            var wp = PlanetDomainExt.GetPolyCell(i, client.Data);
            if (wp == null) return;
            client.Cam().JumpTo(wp.GetCenter());
        });
        this.AddIntButton("Jump to Unit", i =>
        {
            var unit = client.Data.Get<Unit>(i);
            if (unit == null) return;
            client.Cam().JumpTo(unit.Position.GetCell(client.Data).GetCenter());
        });
        this.AddIntButton("Jump to Group", i =>
        {
            var group = client.Data.Get<UnitGroup>(i);
            if (group == null) return;
            client.Cam().JumpTo(group.GetCell(client.Data).GetCenter());
        });
        
        var uiFrame = client.GetComponent<UiFrame>();
        uiFrame.AddTopBar(this);
    }
    public void Process(float delta)
    {
    }
    Node IClientComponent.Node => this;

}
