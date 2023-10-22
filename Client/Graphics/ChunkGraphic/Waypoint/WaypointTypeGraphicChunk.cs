
using System;
using Godot;

public partial class WaypointTypeGraphicChunk : WaypointGraphicChunk
{
    public WaypointTypeGraphicChunk(MapChunk chunk, Data d) : base(chunk, d)
    {
    }
    public override (Color, Color) GetColor(Waypoint wp, Data data)
    {
        Color color;
        if (wp is RiverMouthWaypoint)
        {
            color = Colors.DodgerBlue.Darkened(.4f);
        }
        else if (wp is RiverWaypoint)
        {
            color = Colors.DodgerBlue;
        }
        else if (wp is SeaWaypoint)
        {
            color = Colors.DarkBlue;
        }
        else if (wp is InlandWaypoint n)
        {
            var roughness = n.Roughness;
            color = Colors.White.Darkened(roughness);
        }
        else if (wp is CoastWaypoint)
        {
            color = Colors.Green;
        }
        else
        {
            throw new Exception();
        }

        return (color, color);
    }
    public static ChunkGraphicLayer<WaypointGraphicChunk> GetLayer(Data d, GraphicsSegmenter segmenter)
    {
        var l = new ChunkGraphicLayer<WaypointGraphicChunk>("Waypoint Type", segmenter, 
            c => new WaypointTypeGraphicChunk(c, d), d);
        return l;
    }
}