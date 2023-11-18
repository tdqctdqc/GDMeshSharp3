using System;
using System.Collections.Generic;
using System.Linq;
using Godot;

public partial class MapGraphicsOptions : ScrollContainer, IClientComponent
{
    public Action Disconnect { get; set; }
    public void Process(float delta)
    {
        
    }

    public MapGraphicsOptions(Client client)
    {
        CustomMinimumSize = new Vector2(300f, 600f);
        AnchorsPreset = (int)LayoutPreset.FullRect;
        var vbox = new VBoxContainer();
        vbox.AnchorsPreset = (int)LayoutPreset.FullRect;
        AddChild(vbox);
        client.GetComponent<UiFrame>().LeftSidebar.AddChild(this);
        foreach (var graphicLayer in client.GetComponent<MapGraphics>().GraphicLayerHolder.Layers)
        {
            vbox.AddChild(graphicLayer.GetControl());
            foreach (var setting in graphicLayer.Settings)
            {
                vbox.AddChild(setting.GetControlInterface());
            }
        }
    }

    Node IClientComponent.Node => this;
}
