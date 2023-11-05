using System;
using System.Collections.Generic;
using System.Linq;
using Godot;

public partial class PolyTriFillChunkGraphic : TriColorMesh<PolyTri>
{
    public PolyTriFillChunkGraphic(string name, MapChunk chunk, 
        Func<PolyTri, Data, Color> getColor, Data data) 
        : base(name, getColor,
            (pt, d) =>
            {
                var poly = pt.GetPosition().Poly(d);
                var offset = chunk.RelTo.GetOffsetTo(poly, d);
                return pt.Transpose(offset).Yield();
            }, 
            data => chunk.Polys.SelectMany(p => p.Tris.Tris),
            data)
    {
    }
}
