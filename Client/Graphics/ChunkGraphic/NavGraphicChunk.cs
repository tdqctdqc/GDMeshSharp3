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

        AddWaypointMarkers(nav, mb, chunk, d);
        AddChild(mb.GetMeshInstance());
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
                var n = d.Planet.Nav.Get(nId);
                var nOffset = chunk.RelTo.GetOffsetTo(n.Pos, d);
                mb.AddLine(offset, nOffset, Colors.Red, 2.5f);
            }

            Color color;
            if (point is RiverMouthWaypoint)
            {
                color = Colors.DodgerBlue.Darkened(.4f);
            }
            else if (point is RiverWaypoint)
            {
                color = Colors.DodgerBlue;
            }
            else if (point is SeaWaypoint)
            {
                color = Colors.DarkBlue;
            }
            else if (point is InlandWaypoint n)
            {
                var roughness = n.Roughness;
                color = Colors.White.Darkened(roughness);
            }
            else if (point is CoastWaypoint)
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
