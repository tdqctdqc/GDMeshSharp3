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
                    return p.GetTriangles(chunk.RelTo.Center, d);
                }, d => chunk.Polys, data)
    {
    }
}
