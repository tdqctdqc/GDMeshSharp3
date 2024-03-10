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
        CustomMinimumSize = new Vector2(300f, 600f);
        _scroll = new ScrollContainer();
        _scroll.AnchorsPreset =  (int)LayoutPreset.FullRect;
        MouseFilter = MouseFilterEnum.Stop;
        _scroll.CustomMinimumSize = new Vector2(300f, 600f);
        AddChild(_scroll);
        var vbox = new VBoxContainer();
        _scroll.AddChild(vbox);
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
    }

    public override void _GuiInput(InputEvent @event)
    {
        _scroll._GuiInput(@event);
        GetViewport().SetInputAsHandled();
    }

}
