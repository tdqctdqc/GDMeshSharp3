using Godot;
using System;
using System.Collections.Generic;

public partial class GraphicLayerHolder : Control
{
    private Dictionary<string, GraphicView> _views;
    private Dictionary<string, ButtonToken> _viewTokens;
    private Container _viewButtonsContainer;
    private Container _overlayButtonsContainer;

    public void Setup()
    {
        this.AssignChildNode(ref _viewButtonsContainer, "ViewButtons");
        this.AssignChildNode(ref _overlayButtonsContainer, "OverlayButtons");
        _viewTokens = new Dictionary<string, ButtonToken>();
        _views = new Dictionary<string, GraphicView>();
    }

    public void Clear()
    {
        foreach (var keyValuePair in _views)
        {
            keyValuePair.Value.Clear();
        }
        foreach (var keyValuePair in _viewTokens)
        {
            keyValuePair.Value.Free();
        }
        _viewTokens.Clear();
        _views.Clear();

        while (_viewButtonsContainer.GetChildCount() > 0)
        {
            _viewButtonsContainer.GetChild(0).Free();
        }
    }

    public void AddView(Node2D layer, string name)
    {
        var view = new GraphicView(layer);
        bool vis = true;
        if (_views.Count > 0) vis = false;
            
        view.Toggle(vis);
        _views.Add(name, view);
        var button = new Button();
        button.Text = layer.Visible
            ? "Selected " + name
            : "Turn on " + name;
        _viewButtonsContainer.AddChild(button);
        var token = ButtonToken.CreateToken(button, () => _views[name].Toggle(vis));
        _viewTokens.Add(name, token);
    }

    public void AddOverlay(string viewName, string overlayName, Node2D overlay)
    {
        _views[viewName].AddOverlay(overlay, overlayName, _overlayButtonsContainer);
    }
    private void ToggleLayer(string activeViewName)
    {
        foreach (var keyValuePair in _views)
        {
            var layer = keyValuePair.Value;
            var name = keyValuePair.Key;
            var vis = name == activeViewName;
            _views[name].Toggle(vis);
            _viewTokens[name].Button.Text = vis
                ? "Selected " + name
                : "Turn on " + name;
        }
    }
}
