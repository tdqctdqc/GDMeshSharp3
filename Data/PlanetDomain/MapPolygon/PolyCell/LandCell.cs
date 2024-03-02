
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
        var c = new LandCell(poly.MakeRef(),
            v.MakeRef(), lf.MakeRef(), 
            pre.Geometry,
            pre.Id);
        return c;
    }

    public LandCell(ERef<MapPolygon> polygon,
        ModelRef<Vegetation> vegetation, 
        ModelRef<Landform> landform, 
        CellGeometry geometry,
        int id) 
            : base(
                vegetation, landform, 
                new ERef<Regime>(-1), 
                geometry, id)
    {
        Polygon = polygon;
    }
}