using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using Godot;

public class ChunkGraphicSwitchLayer : IGraphicLayer
{
    public List<IGraphicLayer> Layers { get; private set; }
    
    private IGraphicLayer _showing;
    public string Name { get; private set; }
    
    private bool _visible = true;
    public int Z { get; }
    public List<ISettingsOption> Settings { get; private set; }
    public ChunkGraphicSwitchLayer(int z, string name, params IGraphicLayer[] layers)
    {
        Z = z;
        Name = name;
        Layers = layers.ToList();
        Settings = new List<ISettingsOption>();
        _showing = Layers[0];
        SetLayerVisibilities();
    }
    public bool Visible
    {
        get => _visible;
        set
        {
            if (value)
            {
                foreach (var l in Layers)
                {
                    l.Visible = l == _showing;
                }
            }
            else
            {
                foreach (var l in Layers)
                {
                    l.Visible = false;
                }
            }
        } 
    }


    public Control GetControl()
    {
        var outer = new HBoxContainer();
        var side = new Panel();
        side.CustomMinimumSize = new Vector2(20f, 0f);
        outer.AddChild(side);
        var box = new VBoxContainer();
        outer.AddChild(box);
        
        var btns = ToggleButton.GetToggleGroup(Layers, (l, v) =>
        {
            l.Visible = v;
            if (v) _showing = l;
        });
        for (var i = 0; i < Layers.Count; i++)
        {
            var l = Layers[i];
            var btn = btns.ElementAt(i);
            btn.Text = l.Name;
            box.AddChild(btn);
            foreach (var setting in l.Settings)
            {
                box.AddChild(setting.GetControlInterface());
            }
        }
        
        return outer;
    }
    
    private void SetLayerVisibilities()
    {
        Layers.ForEach(layer =>
        {
            var layerShouldBeVisible = layer == _showing;
            layer.Visible = layerShouldBeVisible;
        });
    }
    public void Update(Data d, ConcurrentQueue<Action> queue)
    {
        foreach (var l in Layers)
        {
            l.Update(d, queue);
        }
    }
}
