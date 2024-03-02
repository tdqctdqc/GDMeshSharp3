using System;
using System.Collections.Generic;
using System.Linq;
using Godot;

public partial class PolyCellFillChunkGraphic : TriColorMesh<Cell>
{
    public PolyCellFillChunkGraphic(string name, MapChunk chunk, 
        Func<Cell, Data, Color> getColor, Data data) 
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
        Func<Cell, bool> isValid,
        Func<Cell, Data, Color> getColor, 
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
