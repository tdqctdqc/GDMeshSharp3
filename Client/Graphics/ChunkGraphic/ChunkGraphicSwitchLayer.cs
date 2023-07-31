using System;
using System.Collections.Generic;
using System.Linq;
using Godot;

public class ChunkGraphicSwitchLayer : IGraphicLayer
{
    public List<IGraphicLayer> Layers { get; private set; }
    private IGraphicLayer _showing;
    public string Name { get; private set; }
    
    private bool _visible = true;
    public List<ISettingsOption> Settings { get; private set; }
    public ChunkGraphicSwitchLayer(string name, params IGraphicLayer[] layers)
    {
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
        var buttons = new Dictionary<Button, IGraphicLayer>();
        Action<bool, Button> setButtonText = (v, b) => b.Text = $"{(v ? "Showing" : "Hiding")} {buttons[b].Name}";

        Layers.ForEach(l =>
        {
            var button = new Button();
            box.AddChild(button);
            button.ButtonUp += () =>
            {
                var visible = (l == _showing) == false;
                if (visible) _showing = l;
                else _showing = null;
                SetLayerVisibilities();
                foreach (var kvp in buttons)
                {
                    setButtonText(kvp.Value == _showing, kvp.Key);
                }
            };
            buttons.Add(button, l);
            setButtonText(l == _showing, button);
            foreach (var setting in l.Settings)
            {
                box.AddChild(setting.GetControlInterface());
            }
        });
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
    public void Update(Data d)
    {
        foreach (var l in Layers)
        {
            l.Update(d);
        }
    }
}
