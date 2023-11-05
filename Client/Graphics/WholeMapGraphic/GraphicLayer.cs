
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using Godot;

public abstract class GraphicLayer<TKey, TGraphic> : IGraphicLayer
    where TGraphic : Node2D
{
    public Dictionary<TKey, TGraphic> Graphics { get; private set; }
    public string Name { get; private set; }
    public List<ISettingsOption> Settings { get; }
    private Dictionary<ISettingsOption, Action<TGraphic>> _settingsUpdaters;
    private Action<TKey, TGraphic, GraphicsSegmenter, ConcurrentQueue<Action>> _updateGraphic;
    private bool _visible = true;
    protected GraphicsSegmenter _segmenter;

    protected GraphicLayer(string name, GraphicsSegmenter segmenter,
        Action<TKey, TGraphic, GraphicsSegmenter, ConcurrentQueue<Action>> updateGraphic)
    {
        _updateGraphic = updateGraphic;
        _segmenter = segmenter;
        Graphics = new Dictionary<TKey, TGraphic>();
        Name = name;
        Settings = new List<ISettingsOption>();
        _settingsUpdaters = new Dictionary<ISettingsOption, Action<TGraphic>>();
        
    }

    public void Add(TKey key, Data d)
    {
        var graphic = GetGraphic(key, d);
        foreach (var kvp in _settingsUpdaters)
        {
            kvp.Value.Invoke(graphic);
        }
        Graphics.Add(key, graphic);
    }
    public void AddSetting<T>(SettingsOption<T> option, 
        Action<TGraphic, T> update)
    {
        option.SettingChanged.SubscribeForNode(() =>
        {
            foreach (var graphic in Graphics.Values)
            {
                update(graphic, option.Value);
            }
        }, _segmenter);
        Settings.Add(option);
        _settingsUpdaters.Add(option, g => update(g, option.Value));
    }
    public void Remove(TKey key)
    {
        var graphic = Graphics[key];
        graphic.QueueFree();
        Graphics.Remove(key);
    }
    public bool Visible
    {
        get => _visible;
        set
        {
            _visible = value;
            foreach (var graphic in Graphics.Values)
            {
                graphic.Visible = _visible;
            }
        } 
    }
    public Control GetControl()
    {
        var button = ButtonExt.GetButton();
        Action<bool> setButtonText = v => button.Text = $"{(v ? "Showing" : "Hiding")} {Name}";
        setButtonText(Visible);
        button.ButtonUp += () =>
        {
            var visible = Visible == false;
            setButtonText(visible);
            Visible = visible;
        };
        return button;
    }
    public void Update(Data d, ConcurrentQueue<Action> queue)
    {
        foreach (var kvp in Graphics)
        {
            var key = kvp.Key;
            var graphic = kvp.Value;
            _updateGraphic(key, graphic, _segmenter, queue);
            queue.Enqueue(() =>
            {
                foreach (var kvp2 in _settingsUpdaters)
                {
                    kvp2.Value.Invoke(graphic);
                }
            });
        }
    }
    protected abstract TGraphic GetGraphic(TKey key, Data d);
}

public static class WholeMapGraphicLayerExt
{
    public static void RegisterForEntityLifetime<TEntity, TGraphic>
        (this GraphicLayer<TEntity, TGraphic> layer, Client client,
            Data d)
        where TGraphic : Node2D where TEntity : Entity
    {
        d.SubscribeForCreation<TEntity>(
            n =>
            {
                if (n.Entity == null) throw new Exception();
                client.QueuedUpdates.Enqueue(() => layer.Add((TEntity)n.Entity, d));
            }
        );
        
        d.SubscribeForDestruction<TEntity>(
            n =>
            {
                client.QueuedUpdates.Enqueue(() => layer.Remove((TEntity)n.Entity));
            }
        );
    }
}