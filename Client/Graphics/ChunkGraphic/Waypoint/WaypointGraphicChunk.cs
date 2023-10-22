using System;
using System.Collections.Generic;
using System.Linq;
using Godot;

public abstract partial class WaypointGraphicChunk : MapChunkGraphicModule
{
    public WaypointGraphicChunk(MapChunk chunk, Data d) 
        : base(chunk, nameof(WaypointGraphicChunk))
    {
        var nav = d.Planet.Nav;
        var mb = new MeshBuilder();

        AddWaypointMarkers(nav, mb, chunk, d);
        AddChild(mb.GetMeshInstance());
    }

    public void Draw(MapChunk chunk, Data d)
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

            var colors = GetColor(point, d);
            mb.AddCircle(offset, 10f, 6, colors.Item2);
            mb.AddCircle(offset, 8f, 6, colors.Item1);
        }
    }

    public abstract (Color inner, Color border) GetColor(Waypoint wp, Data data);
    
}
