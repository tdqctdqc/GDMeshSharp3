
using System.Collections.Generic;
using System.Linq;
using Godot;

public class LandCell : PolyCell
{
    public EntityRef<MapPolygon> Polygon { get; private set; }
    public static LandCell Construct(MapPolygon poly,
        Vector2[] relBoundary, GenWriteKey key)
    {
        var lf = key.Data.Models.Landforms.GetAtPoint(poly, relBoundary.First(), key.Data);
        var v = key.Data.Models.Vegetations.GetAtPoint(poly, relBoundary.First(), lf, key.Data);
        var id = key.Data.IdDispenser.TakeId();
        var c = new LandCell(poly.MakeRef(), poly.Center, relBoundary,
            v.MakeRef(), lf.MakeRef(), new HashSet<int>(), id);
        return c;
    }

    public LandCell(EntityRef<MapPolygon> polygon,
        Vector2 relTo, 
        Vector2[] relBoundary, 
        ModelRef<Vegetation> vegetation, 
        ModelRef<Landform> landform, 
        HashSet<int> neighbors, int id) : base(relTo, relBoundary, vegetation, landform, neighbors, id)
    {
        Polygon = polygon;
    }
}