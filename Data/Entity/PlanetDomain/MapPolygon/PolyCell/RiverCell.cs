
using System.Collections.Generic;
using Godot;

public class RiverCell : PolyCell, IEdgeCell
{
    public EntityRef<MapPolygonEdge> Edge { get; private set; }
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
            v.MakeRef(), lf.MakeRef(), new HashSet<int>(), id);
        return c;
    }
    
    
    public RiverCell(EntityRef<MapPolygonEdge> edge,
        Vector2 relTo, Vector2[] relBoundary, ModelRef<Vegetation> vegetation, ModelRef<Landform> landform, HashSet<int> neighbors, int id) 
            : base(relTo, relBoundary, vegetation, 
                landform, neighbors, 
                new EntityRef<Regime>(-1), id)
    {
        Edge = edge;
    }
}