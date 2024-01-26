
using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using Godot;

public abstract partial class BorderChunkNode 
    : Node2D, IMapChunkGraphicNode
{
    public MapChunk Chunk { get; private set; }
    public string Name { get; private set; }
    Node2D IMapChunkGraphicNode.Node => this;

    public BorderChunkNode(string name, MapChunk chunk, 
        Data data)
    {
        Chunk = chunk;
        Name = name;
        Draw(data);
    }
    private BorderChunkNode() : base()
    {
    }

    protected abstract bool InUnion(MapPolygon p1, MapPolygon p2, Data data);
    protected abstract float GetThickness(MapPolygon p1, MapPolygon p2, Data data);
    protected abstract Color GetColor(MapPolygon p1, Data data);
    
    public void Draw(Data data)
    {
        this.ClearChildren();
        var mb = new MeshBuilder();
        var polys = Chunk.Polys;
        foreach (var element in polys)
        {
            var color = GetColor(element, data);
            var offset = Chunk.RelTo.GetOffsetTo(element, data);
            foreach (var n in element.Neighbors.Items(data))
            {
                if (InUnion(n, element, data)) continue;
                mb.DrawPolyEdge(element, n, p => GetColor(p, data), GetThickness(element, n, data), Chunk.RelTo, data);
            }
        }
        
        if (mb.Tris.Count == 0) return;
        AddChild(mb.GetMeshInstance());
    }
}