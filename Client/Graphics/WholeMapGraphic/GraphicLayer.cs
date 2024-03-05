using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using Godot;

public abstract class GraphicLayer<TKey, TGraphic> : IGraphicLayer
    where TGraphic : Node2D
{
    public Dictionary<TKey, TGraphic> Graphics { get; private set; }
    public abstract MultiSettings GetSettings();
    public string Name { get; private set; }
    public int Z { get; }
    protected GraphicsSegmenter _segmenter;

    protected GraphicLayer(LayerOrder z, string name, 
        GraphicsSegmenter segmenter)
    {
        Z = (int)z;
        _segmenter = segmenter;
        Graphics = new Dictionary<TKey, TGraphic>();
        Name = name;
    }

    public void Add(TKey key, Data d)
    {
        var graphic = GetGraphic(key, d);
        graphic.ZIndex = Z;
        graphic.ZAsRelative = false;
        Graphics.Add(key, graphic);
    }
    public void Remove(TKey key, Data d)
    {
        var graphic = Graphics[key];
        graphic.QueueFree();
        Graphics.Remove(key);
    }
    protected abstract TGraphic GetGraphic(TKey key, Data d);
}

public static class GraphicLayerExt
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