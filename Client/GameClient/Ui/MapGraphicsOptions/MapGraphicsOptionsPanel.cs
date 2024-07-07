using System;
using System.Collections.Generic;
using System.Linq;
using Godot;

public partial class MapGraphicsOptionsPanel 
    : PanelContainer
{
    private ScrollContainer _scroll;
    public void Process(float delta)
    {
        
    }
    
    public MapGraphicsOptionsPanel(Client client)
    {
        var margin = new MarginContainer();
        AddChild(margin);
        var vbox = margin.MakeScroll<VBoxContainer>();
        var first = client.GetComponent<MapGraphics>()
            .GraphicLayerHolder.Chunks.First().Value;
        foreach (var module in first.GetModules())
        {
            var settings = module.GetSettings(client.Data);
            vbox.AddChild(NodeExt.CreateLabel(module.Name));
            foreach (var setting in settings.SettingsOptions)
            {
                var settingControl = setting.GetControlInterface();
                vbox.AddChild(settingControl);
            }
        }
        foreach (var wholeMapGraphic in client.GetComponent<MapGraphics>()
                     .GraphicLayerHolder.WholeMapGraphics)
        {
            var settings = wholeMapGraphic.GetSettings();
            
            vbox.AddChild(NodeExt.CreateLabel(settings.Name));
            foreach (var setting in settings.SettingsOptions)
            {
                var settingControl = setting.GetControlInterface();
                vbox.AddChild(settingControl);
            }
        }
    }
}
