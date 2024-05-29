using System;
using System.Collections.Generic;
using System.Linq;
using Godot;

public partial class MapGraphicsOptionsPanel : Panel
{
    private ScrollContainer _scroll;
    public void Process(float delta)
    {
        
    }
    
    public MapGraphicsOptionsPanel(Client client)
    {
        SelfModulate = Colors.Black;
        var vbox = this.MakeScroll<VBoxContainer>(new Vector2(300f, 600f));
        _scroll = (ScrollContainer)vbox.GetParent();
        var first = client.GetComponent<MapGraphics>()
            .GraphicLayerHolder.Chunks.First().Value;
        foreach (var module in first.GetModules())
        {
            var settings = module.GetSettings(client.Data);
            vbox.AddChild(NodeExt.CreateLabel(module.Name));
            foreach (var setting in settings.SettingsOptions)
            {
                vbox.AddChild(setting.GetControlInterface());
            }
        }
        foreach (var wholeMapGraphic in client.GetComponent<MapGraphics>()
                     .GraphicLayerHolder.WholeMapGraphics)
        {
            var settings = wholeMapGraphic.GetSettings();
            
            vbox.AddChild(NodeExt.CreateLabel(settings.Name));
            foreach (var setting in settings.SettingsOptions)
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

}
