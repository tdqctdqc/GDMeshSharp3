using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Godot;

public abstract partial class MapChunkGraphicNode<TKey> : Node2D, IMapChunkGraphicNode
{
    public MapChunk Chunk { get; private set; }
    public string Name { get; private set; }
    private Dictionary<TKey, Node2D> _graphics;
    private HashSet<TKey> _queuedToAdd;
    private HashSet<TKey> _queuedToChange;
    private HashSet<TKey> _queuedToRemove;
    Node2D IMapChunkGraphicNode.Node => this;
    public MapChunkGraphicNode(string name, Data data, MapChunk chunk)
    {
        Name = name;
        Chunk = chunk;
        _graphics = new Dictionary<TKey, Node2D>();
        _queuedToAdd = new HashSet<TKey>();
        _queuedToRemove = new HashSet<TKey>();
        _queuedToChange = new HashSet<TKey>();
    }
    protected MapChunkGraphicNode() { }
    public override void _ExitTree()
    {
    }

    public void Init(Data data)
    {   
        this.ClearChildren();
        foreach (var kvp in _graphics)
        {
            kvp.Value.QueueFree();
        }
        _graphics.Clear();

        var keys = GetKeys(data);
        foreach (var key in keys)
        {
            Add(key, data);
        }
    }

    public void Update(Data d)
    {
        foreach (var key in _queuedToRemove)
        {
            Remove(key, d);
        }
        _queuedToRemove.Clear();
        foreach (var key in _queuedToChange)
        {
            Change(key, d);
        }
        _queuedToChange.Clear();
        foreach (var key in _queuedToAdd)
        {
            Add(key, d);
        }
        _queuedToAdd.Clear();
    }
    public void QueueAdd(TKey key)
    {
        _queuedToAdd.Add(key);
    }
    public void QueueRemove(TKey key)
    {
        _queuedToRemove.Add(key);
    }
    public void QueueChange(TKey key)
    {
        _queuedToChange.Add(key);
    }
    private void Add(TKey key, Data data)
    {
        if (Ignore(key, data)) return;
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
        Add(key, data);
    }
    private void Remove(TKey key, Data data)
    {
        if (_graphics.ContainsKey(key) == false) return;
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
    
    protected abstract Node2D MakeGraphic(TKey element, Data data);
    protected abstract IEnumerable<TKey> GetKeys(Data data);
    protected abstract bool Ignore(TKey element, Data data);
}

public interface IMapChunkGraphicNode
{
    string Name { get; }
    Node2D Node { get; }
    void Init(Data data);
    void Update(Data d);
}

