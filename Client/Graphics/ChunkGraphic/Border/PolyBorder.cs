
using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using Godot;

public abstract partial class PolyBorder 
    : Node2D, IChunkGraphicModule
{
    public MapChunk Chunk { get; private set; }
    public string Name { get; private set; }
    public Node2D Node => this;
    public PolyBorder(string name, MapChunk chunk, 
        LayerOrder layerOrder,
        Data data)
    {
        Chunk = chunk;
        Name = name;
        ZIndex = (int)layerOrder;
        ZAsRelative = false;
    }
    private PolyBorder() : base()
    {
    }

    protected abstract bool InUnion(MapPolygon p1, MapPolygon p2, Data data);
    protected abstract float GetThickness(MapPolygon p1, MapPolygon p2, Data data);
    protected abstract Color GetColor(MapPolygon p1, Data data);
    public abstract void RegisterForRedraws(Data d);
    public abstract void DoUiTick(UiTickContext context, Data d);

    public void Draw(Data data)
    {
        // this.ClearChildren();
        var mb = MeshBuilder.GetFromPool();
        var polys = Chunk.Polys;
        foreach (var element in polys)
        {
            var color = GetColor(element, data);
            var offset = Chunk.RelTo.GetOffsetTo(element, data);
            foreach (var n in element.Neighbors.Items(data))
            {
                if (InUnion(n, element, data)) continue;
                mb.DrawPolyEdge(element, n, p => GetColor(p, data), GetThickness(element, n, data), Chunk.RelTo.Center, data);
            }
        }
        
        if (mb.TriVertices.Count == 0) return;
        AddChild(mb.GetMeshInstance());
        mb.Return();
    }
}