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
        AddWaypointConnections(chunk, d, nav, mb);

        AddWaypointMarkers(nav, mb, chunk, d);
        AddChild(mb.GetMeshInstance());
    }

    private static void AddWaypointConnections(MapChunk chunk, Data d, Nav nav, MeshBuilder mb)
    {
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
                    mb.AddLine(path1Offset, path2Offset, Colors.Red, 10f);
                }
            }
        }
    }

    private void AddWaypointMarkers(Nav nav, MeshBuilder mb, MapChunk chunk, Data d)
    {
        foreach (var kvp in nav.Waypoints
                     .Where(kvp2 => 
                         chunk.Polys.Contains(d.Get<MapPolygon>(kvp2.Value.AssociatedPolyIds.X))))
        {
            var point = kvp.Value;
            var offset = chunk.RelTo.GetOffsetTo(point.Pos, d);
            foreach (var nId in point.Neighbors)
            {
                try
                {
                    var n = d.Planet.Nav.Waypoints[nId];
                    var nOffset = chunk.RelTo.GetOffsetTo(n.Pos, d);
                    mb.AddLine(offset, nOffset, Colors.Red, 2.5f);
                }
                catch
                {
                    GD.Print("missing waypoint");
                }
            }

            Color color;
            if (point.WaypointData.Value() is RiverNav)
            {
                color = Colors.DodgerBlue;
            }
            else if (point.WaypointData.Value() is SeaNav)
            {
                color = Colors.DarkBlue;
            }
            else if (point.WaypointData.Value() is InlandNav n)
            {
                var roughness = n.Roughness;
                color = Colors.Red.Darkened(roughness);
            }
            else if (point.WaypointData.Value() is CoastNav)
            {
                color = Colors.Green;
            }
            else
            {
                throw new Exception();
            }
            mb.AddCircle(offset, 10f, 6, color);

        }
    }
    public static ChunkGraphicLayer<NavGraphicChunk> GetLayer(Data d, GraphicsSegmenter segmenter)
    {
        var l = new ChunkGraphicLayer<NavGraphicChunk>("Nav", segmenter, 
            c => new NavGraphicChunk(c, d), d);
        return l;
    }
}
