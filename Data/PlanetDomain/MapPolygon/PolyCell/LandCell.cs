
using System.Collections.Generic;
using System.Linq;
using Godot;

public class LandCell : PolyCell, ISinglePolyCell
{
    public ERef<MapPolygon> Polygon { get; private set; }
    public static LandCell Construct(PreCell pre,
        GenWriteKey key)
    {
        var poly = key.Data.Get<MapPolygon>(pre.PrePoly.Id);
        var relTo = pre.RelTo;
        var lf = key.Data.Models.Landforms.GetAtPoint(poly, relTo, key.Data);
        var v = key.Data.Models.Vegetations.GetAtPoint(poly, relTo, lf, key.Data);
        var id = pre.Id;
        var c = new LandCell(poly.MakeRef(), relTo,
            v.MakeRef(), lf.MakeRef(), 
            pre.Neighbors.Select(n => n.Id).ToList(),
            pre.EdgesRel.ToList(), id);
        return c;
    }

    public LandCell(ERef<MapPolygon> polygon,
        Vector2 relTo, 
        ModelRef<Vegetation> vegetation, 
        ModelRef<Landform> landform, 
        List<int> neighbors, 
        List<(Vector2, Vector2)> edges,
        int id) 
            : base(relTo, GetBoundaryPoints(edges),
                vegetation, landform, 
                neighbors, edges, new ERef<Regime>(-1), id)
    {
        Polygon = polygon;
    }
}