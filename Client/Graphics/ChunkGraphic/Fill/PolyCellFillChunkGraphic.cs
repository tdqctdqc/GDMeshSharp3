using System;
using System.Collections.Generic;
using System.Linq;
using Godot;

public partial class PolyCellFillChunkGraphic : TriColorMesh<PolyCell>
{
    public PolyCellFillChunkGraphic(string name, MapChunk chunk, 
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
    public PolyCellFillChunkGraphic(string name, MapChunk chunk, 
        Func<PolyCell, bool> isValid,
        Func<PolyCell, Data, Color> getColor, 
        Data data) 
        : base(name, getColor,
            (pt, d) =>
            {
                var offset = chunk.RelTo.GetOffsetTo(pt.RelTo, d);
                return pt.GetTriangles(chunk.RelTo.Center, d);
            }, 
            data => chunk.Cells.Where(isValid),
            data)
    {
    }
}
