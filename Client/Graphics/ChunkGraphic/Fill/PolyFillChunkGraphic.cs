using System;
using System.Collections.Generic;
using System.Linq;
using Godot;

public partial class PolyFillChunkGraphic : TriColorMesh<MapPolygon>
{
    public PolyFillChunkGraphic(string name, MapChunk chunk, Func<MapPolygon, Data, Color> getColor, Data data) 
            : base(name, getColor,
                (p, d) =>
                {
                    var offset = chunk.RelTo.GetOffsetTo(p, d);
                    return p.Tris.Tris.Select(t => t.Transpose(offset));
                }, d => chunk.Polys, data)
    {
    }
}
