using System;
using System.Collections.Generic;
using System.Linq;
using Godot;

public partial class MapGraphicsOptions : VBoxContainer, IClientComponent
{
    public Action Disconnect { get; set; }
    public void Process(float delta)
    {
        
    }

    public MapGraphicsOptions(Client client)
    {
        client.GetComponent<UiFrame>().LeftSidebar.AddChild(this);
        foreach (var graphicLayer in client.GetComponent<MapGraphics>().GraphicLayerHolder.Layers)
        {
            AddChild(graphicLayer.GetControl());
            foreach (var setting in graphicLayer.Settings)
            {
                AddChild(setting.GetControlInterface());
            }
        }
    }

    Node IClientComponent.Node => this;
}
