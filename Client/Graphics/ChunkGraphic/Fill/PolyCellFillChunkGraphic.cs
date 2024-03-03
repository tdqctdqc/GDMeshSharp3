using System;
using System.Collections.Generic;
using System.Linq;
using Godot;

public abstract partial class PolyCellFillChunkGraphic : TriColorMesh<Cell>
{
    public MapChunk Chunk { get; private set; }
    public PolyCellFillChunkGraphic(string name, 
        MapChunk chunk, 
        GraphicsSegmenter segmenter, 
        LayerOrder layerOrder,
        Data data) 
        : base(name, chunk.RelTo.Center,
            layerOrder, segmenter, data)
    {
        Chunk = chunk;
        DrawFirst(data);
    }

    public override IEnumerable<Triangle> GetTris(Cell e, Data d)
    {
        var offset = Chunk.RelTo.GetOffsetTo(e.RelTo, d);
        return e.GetTriangles(Chunk.RelTo.Center, d);
    }

    public override IEnumerable<Cell> GetElements(Data d)
    {
        return Chunk.Cells.Where(c => IsValid(c, d));
    }

    public abstract bool IsValid(Cell c, Data d);
}
