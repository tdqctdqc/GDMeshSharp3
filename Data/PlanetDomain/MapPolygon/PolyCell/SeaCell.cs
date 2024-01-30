
using System.Collections.Generic;
using Godot;
using MessagePack;

public class SeaCell : PolyCell, ISinglePolyCell
{
    public ERef<MapPolygon> Polygon { get; private set; }
    public static SeaCell Construct(MapPolygon poly,
        Vector2[] relBoundary, GenWriteKey key)
    {
        var lf = key.Data.Models.Landforms.Sea;
        var v = key.Data.Models.Vegetations.Barren;
        var id = key.Data.IdDispenser.TakeId();
        var c = new SeaCell(poly.MakeRef(), poly.Center, relBoundary,
            v.MakeRef(), lf.MakeRef(), new HashSet<int>(), id);
        return c;
    }
    [SerializationConstructor] private SeaCell(
        ERef<MapPolygon> polygon,
        Vector2 relTo, Vector2[] relBoundary, ModelRef<Vegetation> vegetation, ModelRef<Landform> landform, HashSet<int> neighbors, int id) 
            : base(relTo, relBoundary, vegetation, landform, 
                neighbors, new ERef<Regime>(-1), id)
    {
        Polygon = polygon;
    }
}