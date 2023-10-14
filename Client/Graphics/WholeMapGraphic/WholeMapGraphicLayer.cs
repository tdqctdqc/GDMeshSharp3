
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using Godot;

public abstract class WholeMapGraphicLayer<TKey, TGraphic> : IGraphicLayer
    where TGraphic : Node2D
{
    public Dictionary<TKey, TGraphic> Graphics { get; private set; }
    private Node2D _hook;
    public string Name { get; private set; }
    public List<ISettingsOption> Settings { get; }
    private bool _visible = true;
    protected GraphicsSegmenter _segmenter;

    protected WholeMapGraphicLayer(string name, GraphicsSegmenter segmenter, 
        List<ISettingsOption> settings)
    {
        _segmenter = segmenter;
        Graphics = new Dictionary<TKey, TGraphic>();
        Name = name;
        Settings = settings;
    }

    public void Add(TKey key, Data d)
    {
        var graphic = GetGraphic(key, d);
        Graphics.Add(key, graphic);
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
    public abstract void Update(Data d, ConcurrentQueue<Action> queue);
    protected abstract TGraphic GetGraphic(TKey key, Data d);
}

public static class WholeMapGraphicLayerExt
{
    public static void RegisterForEntityLifetime<TEntity, TGraphic>
        (this WholeMapGraphicLayer<TEntity, TGraphic> layer, Data d)
        where TGraphic : Node2D where TEntity : Entity
    {
        d.SubscribeForCreation<TEntity>(
            n => layer.Add((TEntity)n.Entity, d)
        );
        
        d.SubscribeForDestruction<TEntity>(
            n => layer.Remove((TEntity)n.Entity)
        );
    }
}