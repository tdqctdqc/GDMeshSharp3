using System;
using System.Collections.Generic;
using System.Linq;
using Godot;

public partial class NavGraphicChunk : MapChunkGraphicModule
{
    public NavGraphicChunk(MapChunk chunk, Data d) 
        : base(chunk, nameof(NavGraphicChunk))
    {
        var nav = d.Planet.Nav;
        var mb = new MeshBuilder();
        
        foreach (var kvp in nav.Waypoints.Where(kvp2 => chunk.Coords == kvp2.Value.ChunkCoords))
        {
            var point = kvp.Value;
            var offset = chunk.RelTo.GetOffsetTo(point.Pos, d);
            foreach (var nId in point.Neighbors)
            {
                var n = d.Planet.Nav.Waypoints[nId];
                var nOffset = chunk.RelTo.GetOffsetTo(n.Pos, d);
                // if(nOffset.Length() == 0f) GD.Print("close " + point.Pos);
                mb.AddLine(offset, nOffset, Colors.Red, 2f);
            }
            mb.AddCircle(offset, 10f, 6, Colors.Red);

        }
        
        foreach (var poly in chunk.Polys)
        {
            var point = nav.GetPolyWaypoint(poly);
            var offset = chunk.RelTo.GetOffsetTo(point.Pos, d);

            foreach (var nId in point.Neighbors)
            {
                // var n = d.Planet.Nav.Waypoints[nId];
                // var nOffset = chunk.RelTo.GetOffsetTo(n.Pos, d);
                // // if(nOffset.Length() == 0f) GD.Print("close " + point.Pos);
                // mb.AddLine(offset, nOffset, ColorsExt.GetRainbowColor(nId), 2f);
            }
        }
        
        foreach (var poly in chunk.Polys)
        {
            if (poly.IsWater()) continue;
            var ns = poly.Neighbors.Items(d);
            foreach (var n in ns)
            {
                if (n.IsWater()) continue;
                var path = nav.GetPolyPath(poly, n);
                if (path == null) continue;
                for (var i = 0; i < path.Count() - 1; i++)
                {
                    // GD.Print("adding graphic");
                    var path1Offset = chunk.RelTo.GetOffsetTo(path.ElementAt(i).Pos, d);
                    var path2Offset = chunk.RelTo.GetOffsetTo(path.ElementAt(i + 1).Pos, d);
                    mb.AddLine(path1Offset, path2Offset, Colors.Blue, 5f);
                }
            }
        }
        AddChild(mb.GetMeshInstance());
    }

    public static ChunkGraphicLayer<NavGraphicChunk> GetLayer(Data d, GraphicsSegmenter segmenter)
    {
        var l = new ChunkGraphicLayer<NavGraphicChunk>("Nav", segmenter, 
            c => new NavGraphicChunk(c, d), d);
        return l;
    }
}
