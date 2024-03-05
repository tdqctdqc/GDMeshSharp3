using System;
using System.Collections.Generic;
using System.Linq;
using Godot;

public abstract partial class PolyCellFillChunkGraphic : TriColorMesh<Cell>
{
    public MapChunk Chunk { get; private set; }
    public PolyCellFillChunkGraphic(string name, 
        MapChunk chunk, 
        LayerOrder layerOrder,
        Vector2 zoomVisRange,
        Data data) 
        : base(name, chunk.RelTo.Center,
            layerOrder,
            chunk.CellTriCounts, chunk.Cells, chunk.CellTriVertices,
            zoomVisRange,
            data)
    {
        Chunk = chunk;
    }
}
