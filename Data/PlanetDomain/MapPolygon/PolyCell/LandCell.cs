
using System.Collections.Generic;
using System.Linq;
using Godot;
using VoronoiSandbox;

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
            GetBoundaryPoints(pre.EdgesRel),
            v.MakeRef(), lf.MakeRef(), 
            pre.Neighbors.Select(n => n.Id).ToList(),
            pre.EdgesRel.Select(e => ((Vector2)e.Item1, (Vector2)e.Item2)).ToList(), pre.Id);
        return c;
    }

    public LandCell(ERef<MapPolygon> polygon,
        Vector2 relTo, Vector2[] relBoundary, 
        ModelRef<Vegetation> vegetation, 
        ModelRef<Landform> landform, 
        List<int> neighbors, 
        List<(Vector2, Vector2)> edges,
        int id) 
            : base(relTo, relBoundary,
                vegetation, landform, 
                neighbors, edges, new ERef<Regime>(-1), id)
    {
        Polygon = polygon;
    }
}