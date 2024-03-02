
using System.Collections.Generic;
using System.Linq;
using Godot;
using MessagePack;
using VoronoiSandbox;

public class SeaCell : PolyCell, ISinglePolyCell
{
    public ERef<MapPolygon> Polygon { get; private set; }
    
    
    public static SeaCell Construct2(MapPolygon poly, GenWriteKey key)
    {
        var preCells = key.GenData.GenAuxData.PreCellPolys[poly];
        var edges = new List<(Vector2, Vector2)>();
        var nIds = new List<int>();
        for (var i = 0; i < preCells.Count; i++)
        {
            var c = preCells[i];
            for (var j = 0; j < c.Neighbors.Count; j++)
            {
                var n = c.Neighbors[j];
                if (n.PrePoly.Id == poly.Id) continue;
                var edge = c.EdgesRel[j];
                edges.Add((
                    poly.Center.Offset(edge.Item1 + c.RelTo, key.Data),
                    poly.Center.Offset(edge.Item2 + c.RelTo, key.Data)
                ));
                nIds.Add(n.Id);
            }
        }

        var boundary = edges.Select(e => new LineSegment(e.Item1, e.Item2))
            .ToList().FlipChainify().GetPoints().ToArray();
        var lf = key.Data.Models.Landforms.Sea;
        var v = key.Data.Models.Vegetations.Barren;
        var id = key.Data.IdDispenser.TakeId();
        var cell = new SeaCell(poly.MakeRef(),
            poly.Center, boundary, v.MakeRef(), 
            lf.MakeRef(),
            nIds, edges, id);

        return cell;
    }
    
    
    public static SeaCell Construct(PreCell pre, GenWriteKey key)
    {
        var poly = key.Data.Get<MapPolygon>(pre.PrePoly.Id);
        var relTo = pre.RelTo;
        var lf = key.Data.Models.Landforms.Sea;
        var v = key.Data.Models.Vegetations.Barren;
        var c = new SeaCell(poly.MakeRef(), relTo, 
            GetBoundaryPoints(pre.EdgesRel),
            v.MakeRef(), lf.MakeRef(), 
            pre.Neighbors.Select(n => n.Id).ToList(),
            pre.EdgesRel.Select(e => ((Vector2)e.Item1, (Vector2)e.Item2)).ToList(), pre.Id);
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