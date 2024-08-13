
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Godot;

public abstract class Issue
{
    public string Message { get; set; }
    public int Tick { get; private set; }
    public Vector2 Pos { get; private set; }
    private List<string> _layers;
    private Dictionary<string, Action<MeshBuilder>> _layerActions;
    private Dictionary<string, MapOverlayDrawer> _overlays;
    
    protected Issue(Vector2 pos, string message, int tick)
    {
        Tick = tick;
        Pos = pos;
        Message = message;
        _layers = new List<string>();
        _layerActions = new Dictionary<string, Action<MeshBuilder>>();
    }

    protected void AddLayer(string name, Action<MeshBuilder> action)
    {
        _layers.Add(name);
        _layerActions.Add(name, action);
    }
    
    public void Draw(Client c)
    {
        if (_overlays is null)
        {
            _overlays = _layerActions
                .ToDictionary(kvp => kvp.Key,
                    kvp => c.GetComponent<MapGraphics>().GetOverlay(LayerOrder.Debug));
        }
        else
        {
            foreach (var (key, value) in _overlays)
            {
                c.GetComponent<MapGraphics>().RemoveOverlay(value);
            }
        }

        
        for (var i = 0; i < _layers.Count; i++)
        {
            var name = _layers[i];
            var overlay = _overlays[name];
            overlay.Draw(_layerActions[name], Pos);
        }
    }


    public void Clear(Client c)
    {
        if (_overlays is not null)
        {
            foreach (var (key, value) in _overlays)
            {
                c.GetComponent<MapGraphics>().RemoveOverlay(value);
            }
        }
    }

    public ISettings GetSettings()
    {
        var settings = new Settings(Message);
        foreach (var (name, action) in _layerActions)
        {
            var n = name;
            var setting = new BoolSettingsOption($"{name} visibility",
                true);
            setting.SettingChanged.Subscribe(v =>
            {
                var overlay = _overlays[n];
                overlay.SetVisibility(v.newVal);
            });
            settings.SettingsOptions.Add(setting);
        }

        return settings;
    }
    
    
}