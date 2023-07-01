using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Godot;

public partial class MapDisplayOptionsUi : VBoxContainer
{
    private ButtonToken _roads, _regimes, _landforms, _vegetation;
    private Label _mousePos;
    private Data _data;
    public override void _Ready()
    {
        
    }

    public override void _Process(double delta)
    {
        _mousePos.Text = Game.I.Client?.Cam?.GetMousePosInMapSpace().ToString();
    }

    public void Setup(MapGraphics graphics, Data data)
    {
        _data = data;
        _mousePos = new Label();
        AddChild(_mousePos);
        var chunkFactories = typeof(MapChunkGraphic)
            .GetProperties(BindingFlags.Static | BindingFlags.Public)
            .Where(p => p.PropertyType == typeof(ChunkGraphicFactory))
            .Select(p => (ChunkGraphicFactory)p.GetMethod.Invoke(null, null));
        
        foreach (var pi in chunkFactories)
        {
            if (pi.Active == false) continue;
            var name = pi.Name;
            var btn = new Button();
            btn.Text = "Showing " + name;
            Action toggle = () =>
            {
                foreach (var mc in graphics.MapChunkGraphics)
                {
                    var n = mc.Modules[name];
                    Toggle(mc, n, btn, name);
                }
            };
            
            var token = ButtonToken.CreateToken(btn, toggle);
            AddChild(btn);
        }
    }
    private void Toggle(MapChunkGraphic mc, Node2D n,  Button btn, string name)
    {
        bool vis = n.Toggle();
        btn.Text = vis
            ? "Showing " + name
            : name + " is hidden";
    }
}