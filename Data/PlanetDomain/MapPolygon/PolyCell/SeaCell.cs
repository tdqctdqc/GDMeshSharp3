
using System.Collections.Generic;
using System.Linq;
using Godot;
using MessagePack;

public class SeaCell : PolyCell, ISinglePolyCell
{
    public ERef<MapPolygon> Polygon { get; private set; }
    public static SeaCell Construct(PreCell pre, GenWriteKey key)
    {
        var poly = key.Data.Get<MapPolygon>(pre.PrePoly.Id);
        var relTo = pre.RelTo;
        var lf = key.Data.Models.Landforms.Sea;
        var v = key.Data.Models.Vegetations.Barren;
        var c = new SeaCell(poly.MakeRef(), poly.Center, 
            GetBoundaryPoints(pre.EdgesRel),
            v.MakeRef(), lf.MakeRef(), 
            pre.Neighbors.Select(n => n.Id).ToList(),
            pre.EdgesRel.ToList(), pre.Id);
        return c;
    }
    [SerializationConstructor] private SeaCell(
        ERef<MapPolygon> polygon,
        Vector2 relTo, Vector2[] relBoundary, 
        ModelRef<Vegetation> vegetation, 
        ModelRef<Landform> landform, 
        List<int> neighbors, 
        List<(Vector2, Vector2)> edges,
        int id) 
            : base(relTo, relBoundary, vegetation, landform, 
                neighbors, edges, new ERef<Regime>(-1), id)
    {
        Polygon = polygon;
    }
}