
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using Godot;

public abstract class GraphicLayer<TKey, TGraphic> : IGraphicLayer
    where TGraphic : Node2D
{
    public Dictionary<TKey, TGraphic> Graphics { get; private set; }
    public string Name { get; private set; }
    public int Z { get; }
    public List<ISettingsOption> Settings { get; }
    private Dictionary<ISettingsOption, Action<TGraphic>> _settingsUpdaters;
    private Action<TKey, TGraphic, GraphicsSegmenter, ConcurrentQueue<Action>> _updateGraphic;
    private bool _visible = true;
    protected GraphicsSegmenter _segmenter;

    protected GraphicLayer(int z, string name, GraphicsSegmenter segmenter,
        Action<TKey, TGraphic, GraphicsSegmenter, ConcurrentQueue<Action>> updateGraphic)
    {
        Z = z;
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
        graphic.ZIndex = Z;
        graphic.ZAsRelative = false;
        foreach (var kvp in _settingsUpdaters)
        {
            kvp.Value.Invoke(graphic);
        }
        Graphics.Add(key, graphic);
        graphic.Visible = _visible;
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
    public void Remove(TKey key, Data d)
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
                if (Graphics.ContainsKey(key) == false) return;
                foreach (var kvp2 in _settingsUpdaters)
                {
                    kvp2.Value.Invoke(graphic);
                }
            });
        }
    }
    protected abstract TGraphic GetGraphic(TKey key, Data d);
    public void EnforceSettings()
    {
        foreach (var graphic in Graphics.Values)
        {
            foreach (var kvp in _settingsUpdaters)
            {
                kvp.Value.Invoke(graphic);
            }
        }
    }
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
                client.QueuedUpdates.Enqueue(() => layer.Remove((TEntity)n.Entity, d));
            }
        );

        var entities = d.GetAll<TEntity>();
        foreach (var entity in entities)
        {
            layer.Add(entity, d);
        }
    }
    
}