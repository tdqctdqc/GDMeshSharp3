using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using Godot;

public abstract partial class WaypointGraphicChunk : Node2D, IMapChunkGraphicNode
{
    private MapChunk _chunk;
    public string Name { get; private set; }
    Node2D IMapChunkGraphicNode.Node => this;
    private Dictionary<Waypoint, int> _ids;
    private MultiMeshInstance2D _inner, _outer;
    private Func<Waypoint, Data, (Color, Color)> _getColor; 
    protected bool _stale;
    public WaypointGraphicChunk(MapChunk chunk, Data d)
    {
        _chunk = chunk;
        _stale = false;
    }

    public void MarkStale()
    {
        _stale = true;
    }
    
    public abstract (Color inner, Color border) GetColor(Waypoint wp, Data data);

    public void Init(Data data)
    {
        var nav = data.Planet.Nav;
        var chunkWps = nav.Waypoints
            .Values
            .Where(wp => _chunk.Polys.Contains(data.Get<MapPolygon>(wp.AssociatedPolyIds.X)))
            .ToList();
        _ids = new Dictionary<Waypoint, int>();
        _outer = new MultiMeshInstance2D();
        _outer.Multimesh = new MultiMesh();
        var outerQuad = new QuadMesh();
        outerQuad.Size = 12f * Vector2.One;
        _outer.Multimesh.Mesh = outerQuad;
        _outer.Multimesh.UseColors = true;
        AddChild(_outer);
        _outer.Multimesh.InstanceCount = chunkWps.Count();
        
        _inner = new MultiMeshInstance2D();
        _inner.Multimesh = new MultiMesh();
        var innerQuad = new QuadMesh();
        innerQuad.Size = 10f * Vector2.One;
        _inner.Multimesh.Mesh = innerQuad;
        _inner.Multimesh.UseColors = true;
        AddChild(_inner);
        _inner.Multimesh.InstanceCount = chunkWps.Count();
        for (var i = 0; i < chunkWps.Count; i++)
        {
            var wp = chunkWps[i];
            var colors = GetColor(wp, data);
            var offset = _chunk.RelTo.GetOffsetTo(wp.Pos, data);
            _inner.Multimesh.SetInstanceColor(i, colors.inner);
            _inner.Multimesh.SetInstanceTransform2D(i, new Transform2D(0f, offset));
            _outer.Multimesh.SetInstanceColor(i, colors.border);
            _outer.Multimesh.SetInstanceTransform2D(i, new Transform2D(0f, offset));
            _ids.Add(wp, i);
        }
    }
    public void Update(Data d, ConcurrentQueue<Action> queue)
    {
        if (_stale)
        {
            queue.Enqueue(() => Update(d));
            _stale = false;
        }
    }
    private void Update(Data data)
    {
        foreach (var kvp in _ids)
        {
            var wp = kvp.Key;
            var id = kvp.Value;
            var colors = GetColor(wp, data);
            _inner.Multimesh.SetInstanceColor(id, colors.inner);
            _outer.Multimesh.SetInstanceColor(id, colors.border);
        }
    }
}
