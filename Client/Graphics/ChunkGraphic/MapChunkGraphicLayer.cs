using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Godot;

public class MapChunkLayerBenchmark
{
    public static Dictionary<Type, ConcurrentBag<int>> Times = new Dictionary<Type, ConcurrentBag<int>>();
}
public abstract partial class MapChunkGraphicLayer<TKey> : Node2D, IMapChunkGraphicLayer
{
    public MapChunk Chunk { get; private set; }
    private Dictionary<TKey, Node2D> _graphics;
    private ChunkChangeListener<TKey> _listener;
    private RefAction<TKey> _add, _change, _remove;
    Node2D IMapChunkGraphicLayer.Node => this;
    public MapChunkGraphicLayer(Data data, MapChunk chunk, ChunkChangeListener<TKey> listener)
    {
        _listener = listener;
        Chunk = chunk;
        _graphics = new Dictionary<TKey, Node2D>();
        _add = new RefAction<TKey>();
        _add.Subscribe(k => Add(k, data));
        _change = new RefAction<TKey>();
        _change.Subscribe(k => Change(k, data));
        _remove = new RefAction<TKey>();
        _remove.Subscribe(k => Remove(k, data));

        if (_listener != null)
        {
            _listener.Added[chunk].Subscribe(_add);
            _listener.Changed[chunk].Subscribe(_change);
            _listener.Removed[chunk].Subscribe(_remove);
        }
    }
    protected MapChunkGraphicLayer()
    {
        
    }

    public override void _ExitTree()
    {
        if (_listener != null)
        {
            _listener.Added[Chunk].Unsubscribe(_add);
            _listener.Changed[Chunk].Unsubscribe(_change);
            _listener.Removed[Chunk].Unsubscribe(_remove);
            _add.Clear();
            _change.Clear();
            _remove.Clear();
        }
    }

    public void Init(Data data)
    {
        this.ClearChildren();
        _graphics.Clear();
        
        if (MapChunkLayerBenchmark.Times.ContainsKey(GetType()) == false)
        {
            MapChunkLayerBenchmark.Times.Add(GetType(), new ConcurrentBag<int>());
        }

        var sw = new Stopwatch();
        sw.Start();
        var keys = GetKeys(data);
        foreach (var key in keys)
        {
            Add(key, data);
        }
        sw.Stop();
        MapChunkLayerBenchmark.Times[GetType()].Add((int)sw.Elapsed.TotalMilliseconds);
    }

    private void Add(TKey key, Data data)
    {
        var graphic = MakeGraphic(key, data);
        _graphics.Add(key, graphic);
        AddChild(graphic);
    }
    private void Change(TKey key, Data data)
    {
        if (_graphics.ContainsKey(key))
        {
            _graphics[key].QueueFree();
            _graphics.Remove(key);
        }
        var graphic = MakeGraphic(key, data);
        _graphics.Add(key, graphic);
        AddChild(graphic);
    }
    private void Remove(TKey key, Data data)
    {
        if (IsInstanceValid(this) == false) return;

        var graphic = _graphics[key];
        graphic.QueueFree();
        _graphics.Remove(key);
    }

    protected void SetRelPos(Node2D node, PolyTriPosition pos, Data data)
    {
        var poly = pos.Poly(data);
        var offset = Chunk.RelTo.GetOffsetTo(poly, data);
        node.Position = offset + pos.Tri(data).GetCentroid();
    }
    protected void SetRelPos(Node2D node, MapPolygon poly, Data data)
    {
        node.Position = Chunk.RelTo.GetOffsetTo(poly, data);
    }
    
    protected abstract Node2D MakeGraphic(TKey key, Data data);
    protected abstract IEnumerable<TKey> GetKeys(Data data);
}

public interface IMapChunkGraphicLayer
{
    Node2D Node { get; }
    void Init(Data data);
}
