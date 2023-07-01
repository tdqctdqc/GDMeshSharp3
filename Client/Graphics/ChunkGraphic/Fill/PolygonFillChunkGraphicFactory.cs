
using System;
using System.Linq;
using Godot;

public class PolygonFillChunkGraphicFactory : ChunkGraphicFactory
{
    private Func<MapPolygon, Data, Color> _getPolyColor;
    private Func<PolyTri, Data, Color> _getTriColor;
    public PolygonFillChunkGraphicFactory(string name, bool active, Func<MapPolygon, Data, Color> getPolyColor) 
        : base(name, active)
    {
        _getPolyColor = getPolyColor;
    }
    public override MapChunkGraphicModule GetModule(MapChunk c, Data d, MapGraphics mg)
    {
        return new PolyFillChunkGraphic(c, d, 
            p => _getPolyColor(p, d));
    }
}
