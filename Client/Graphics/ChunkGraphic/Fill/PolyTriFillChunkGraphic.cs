using System;
using System.Collections.Generic;
using System.Linq;
using Godot;

public partial class PolyTriFillChunkGraphic : TriColorMesh<PolyCell>
{
    public PolyTriFillChunkGraphic(string name, MapChunk chunk, 
        Func<PolyCell, Data, Color> getColor, Data data) 
        : base(name, getColor,
            (pt, d) =>
            {
                var offset = chunk.RelTo.GetOffsetTo(pt.RelTo, d);
                return pt.GetTriangles(chunk.RelTo.Center, d);
            }, 
            data => chunk.Cells,
            data)
    {
    }
}
