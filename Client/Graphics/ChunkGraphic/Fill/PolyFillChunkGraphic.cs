using System;
using System.Collections.Generic;
using System.Linq;
using Godot;

public abstract partial class PolyFillChunkGraphic : TriColorMesh<MapPolygon>
{
    public MapChunk Chunk { get; private set; }
    public PolyFillChunkGraphic(string name, MapChunk chunk, 
        LayerOrder layerOrder, GraphicsSegmenter segmenter, 
        Data data) 
            : base(name, chunk.RelTo.Center, layerOrder, segmenter, data)
    {
        Chunk = chunk;
        DrawFirst(data);
    }
    public override IEnumerable<Triangle> GetTris(MapPolygon e, Data d)
    {
        return e.GetTriangles(Chunk.RelTo.Center, d);
    }
    public override IEnumerable<MapPolygon> GetElements(Data d)
    {
        return Chunk.Polys;
    }
}
