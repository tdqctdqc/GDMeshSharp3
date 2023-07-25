using System;
using System.Collections.Generic;
using System.Linq;
using Godot;

public class ChunkGraphicSwitchLayer : IGraphicLayer
{
    public List<IGraphicLayer> Layers { get; private set; }
    private IGraphicLayer _showing;
    public string Name { get; private set; }
    public ChunkGraphicSwitchLayer(string name, params IGraphicLayer[] layers)
    {
        Name = name;
        Layers = layers.ToList();
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

    private bool _visible = true;
    public Control GetControl()
    {
        var hbox = new HBoxContainer();
        var buttons = new Dictionary<Button, IGraphicLayer>();
        Action<bool, Button> setButtonText = (v, b) => b.Text = $"{(v ? "Showing" : "Hiding")} {buttons[b].Name}";

        Layers.ForEach(l =>
        {
            var button = new Button();
            hbox.AddChild(button);
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
        });
        return hbox;
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
        _showing?.Update(d);
    }

}
