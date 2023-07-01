using System;
using System.Collections.Generic;
using System.Linq;
using Godot;

public class GraphicView
{
    private Node2D _node;
    private Dictionary<string, Node2D> _overlays;
    private Dictionary<string, ButtonToken> _overlayTokens;

    public GraphicView(Node2D node)
    {
        _node = node;
        _overlays = new Dictionary<string, Node2D>();
        _overlayTokens = new Dictionary<string, ButtonToken>();
    }

    public void Toggle(bool vis)
    {
        _node.Visible = vis;
        foreach (var keyValuePair in _overlayTokens)
        {
            keyValuePair.Value.Button.Visible = vis;
        }
    }

    public void AddOverlay(Node2D overlay, string name, Container overlayButtonsContainer)
    {
        _overlays.Add(name, overlay);
        var button = new Button();
        button.Text = "Hide " + name;
        var token = ButtonToken.CreateToken(button, () => ToggleOverlay(name));
        _overlayTokens.Add(name, token);
        overlayButtonsContainer.AddChild(button);
        
    }

    private void ToggleOverlay(string name)
    {
        var vis = !_overlays[name].Visible;
        _overlays[name].Visible = vis;
        _overlayTokens[name].Button.Text = (vis ? "Hide " : "Show ") + name;
    }
    public void Clear()
    {
        foreach (var keyValuePair in _overlays)
        {
            keyValuePair.Value.Free();
        }

        foreach (var keyValuePair in _overlayTokens)
        {
            keyValuePair.Value.Button.Free();
        }
    }
    
}