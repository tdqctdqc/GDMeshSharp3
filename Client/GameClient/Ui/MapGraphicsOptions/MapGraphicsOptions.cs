using System;
using System.Collections.Generic;
using System.Linq;
using Godot;

public partial class MapGraphicsOptions : Panel, IClientComponent
{
    public Action Disconnect { get; set; }
    private ScrollContainer _scroll;
    public void Process(float delta)
    {
        
    }

    public MapGraphicsOptions(Client client)
    {
        SelfModulate = Colors.Black;
        CustomMinimumSize = new Vector2(300f, 600f);
        AnchorsPreset = (int)LayoutPreset.FullRect;
        _scroll = new ScrollContainer();
        _scroll.AnchorsPreset =  (int)LayoutPreset.FullRect;
        MouseFilter = MouseFilterEnum.Stop;
        _scroll.CustomMinimumSize = new Vector2(300f, 600f);
        AddChild(_scroll);
        var vbox = new VBoxContainer();
        vbox.AnchorsPreset = (int)LayoutPreset.FullRect;
        _scroll.AddChild(vbox);
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

    public override void _GuiInput(InputEvent @event)
    {
        _scroll._GuiInput(@event);
        GetViewport().SetInputAsHandled();
    }

    Node IClientComponent.Node => this;
}
