using System;
using System.Collections.Generic;
using System.Linq;
using Godot;

public class ChunkGraphicLayer<TGraphic> : IGraphicLayer
    where TGraphic : Node2D, IMapChunkGraphicNode
{
    public string Name { get; private set; }
    public Dictionary<Vector2, TGraphic> ByChunkCoords { get; private set; }
    private bool _visible = true;
    private GraphicsSegmenter _segmenter;
    public ChunkGraphicLayer(string name, GraphicsSegmenter segmenter,
        Func<MapChunk, TGraphic> getGraphic, Data data)
    {
        Name = name;
        _segmenter = segmenter;
        ByChunkCoords = new Dictionary<Vector2, TGraphic>();
        foreach (var chunk in data.Planet.PolygonAux.Chunks)
        {
            var graphic = getGraphic(chunk);
            graphic.Init(data);
            ByChunkCoords.Add(chunk.Coords, graphic);
            segmenter.AddElement(graphic, chunk.RelTo.Center);
        }
    }
    public bool Visible
    {
        get => _visible;
        set
        {
            _visible = value;
            foreach (var graphic in ByChunkCoords.Values)
            {
                graphic.Visible = _visible;
            }
        } 
    }
    public Control GetControl()
    {
        var button = new Button();
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
    public void Update(Data d)
    {
        foreach (var graphic in ByChunkCoords.Values)
        {
            graphic.Update(d);
        }
    }


    public void RegisterForNotice<TNotice>(RefAction<TNotice> refAction,
        Func<TNotice, MapChunk> getChunk, Action<TNotice, TGraphic> queueUpdate)
    {
        refAction.SubscribeForNode(n =>
        {
            var graphic = ByChunkCoords[getChunk(n).Coords];
            queueUpdate(n, graphic);
        }, _segmenter);
    }
}

public static class MapChunkGraphicNodeExt
{
    public static void RegisterForEntityLifetime<TEntity, TGraphic>(this ChunkGraphicLayer<TGraphic> l, 
        Func<TEntity, MapChunk> getChunk, Func<TGraphic,  MapChunkGraphicNode<TEntity>> getNode, Data d) 
        where TGraphic : Node2D, IMapChunkGraphicNode where TEntity : Entity
    {
        l.RegisterForNotice(d.GetEntityTypeNode<TEntity>().Created, 
            n => getChunk((TEntity)n.Entity),
            (n, graphic) =>
            {
                var node = getNode(graphic);
                node.QueueAdd((TEntity)n.Entity);
            });
        
        l.RegisterForNotice(d.GetEntityTypeNode<TEntity>().Destroyed, 
            n => getChunk((TEntity)n.Entity),
            (n, graphic) =>
            {
                var node = getNode(graphic);
                node.QueueRemove((TEntity)n.Entity);
            });
    }
    
    public static void RegisterForAdd<TKey, TGraphic>(this ChunkGraphicLayer<TGraphic> l,
        RefAction<TKey> action, 
        Func<TKey, MapChunk> getChunk, 
        Func<TGraphic, MapChunkGraphicNode<TKey>> getNode) 
        where TGraphic : Node2D, IMapChunkGraphicNode
    {
        l.RegisterForNotice(action, 
            n => getChunk(n),
            (n, graphic) =>
            {
                var node = getNode(graphic);
                node.QueueAdd(n);
            });
    }
    
    public static void RegisterForRemove<TKey, TGraphic>(this ChunkGraphicLayer<TGraphic> l,
        RefAction<TKey> action, 
        Func<TKey, MapChunk> getChunk, 
        Func<TGraphic, MapChunkGraphicNode<TKey>> getNode) 
        where TGraphic : Node2D, IMapChunkGraphicNode
    {
        l.RegisterForNotice(action, 
            n => getChunk(n),
            (n, graphic) =>
            {
                var node = getNode(graphic);
                node.QueueRemove(n);
            });
    }
}