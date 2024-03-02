
using System.Collections.Generic;
using System.Linq;
using Godot;
using MessagePack;
using VoronoiSandbox;

public class SeaCell : PolyCell, ISinglePolyCell
{
    public ERef<MapPolygon> Polygon { get; private set; }
    
    public static SeaCell Construct(PreCell pre, GenWriteKey key)
    {
        var poly = key.Data.Get<MapPolygon>(pre.PrePoly.Id);
        var relTo = pre.RelTo;
        var lf = key.Data.Models.Landforms.Sea;
        var v = key.Data.Models.Vegetations.Barren;
        var c = new SeaCell(poly.MakeRef(), 
            v.MakeRef(), lf.MakeRef(), 
            pre.Geometry,
            pre.Id);
        return c;
    }
    [SerializationConstructor] private SeaCell(
        ERef<MapPolygon> polygon,
        ModelRef<Vegetation> vegetation, 
        ModelRef<Landform> landform, 
        CellGeometry geometry,
        int id) 
            : base(vegetation, landform, 
                new ERef<Regime>(-1), 
                geometry, id)
    {
        Polygon = polygon;
    }
}