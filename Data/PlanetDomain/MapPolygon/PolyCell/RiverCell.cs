
using System.Collections.Generic;
using Godot;

public class RiverCell : PolyCell, IEdgeCell
{
    public ERef<MapPolygonEdge> Edge { get; private set; }
    public static RiverCell Construct(MapPolygonEdge edge,
        Vector2 relTo,
        Vector2[] relBoundary, GenWriteKey key)
    {
        var lf = key.Data.Models.Landforms.Sea;
        var v = key.Data.Models.Vegetations.Barren;
        var id = key.Data.IdDispenser.TakeId();
        var c = new RiverCell(
            edge.MakeRef(), 
            relTo, relBoundary,
            v.MakeRef(), lf.MakeRef(), 
            new List<int>(),
            new List<(Vector2, Vector2)>(),
            id);
        return c;
    }
    
    
    public RiverCell(ERef<MapPolygonEdge> edge,
        Vector2 relTo, Vector2[] relBoundary, 
        ModelRef<Vegetation> vegetation, 
        ModelRef<Landform> landform, 
        List<int> neighbors, 
        List<(Vector2, Vector2)> edges,
        int id) 
            : base(relTo, relBoundary, vegetation, 
                landform, neighbors, edges,
                new ERef<Regime>(-1), id)
    {
        Edge = edge;
    }


    public void MakeNeighbors(GenWriteKey key)
    {
        
    }
}