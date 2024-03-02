
using System.Collections.Generic;
using System.Linq;
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


    public void MakeNeighbors(Vector2I edgeKey,
        Dictionary<Vector2I, RiverCell> dic,
        GenWriteKey key)
    {
        var bank1 = PlanetDomainExt.GetPolyCell(edgeKey.X, key.Data);
        var bank2 = PlanetDomainExt.GetPolyCell(edgeKey.Y, key.Data);
        Neighbors.Add(bank1.Id);
        bank1.Neighbors.Add(Id);
        Edges.Add(default);
        bank1.Edges.Add(default);
        
        Neighbors.Add(bank2.Id);
        bank2.Neighbors.Add(Id);
        Edges.Add(default);
        bank2.Edges.Add(default);
        
        var mutuals = bank1.Neighbors
            .Where(i => bank2.Neighbors.Contains(i));
        foreach (var mutual in mutuals)
        {
            var mutualCell = PlanetDomainExt.GetPolyCell(mutual, key.Data);
            if (mutualCell is RiverCell) continue;
            var mutualKey1 = edgeKey.X.GetIdEdgeKey(mutual);
            if (dic.TryGetValue(mutualKey1, out var rSeg1))
            {
                Neighbors.Add(rSeg1.Id);
                Edges.Add(default);
            }
            var mutualKey2 = edgeKey.Y.GetIdEdgeKey(mutual);
            if (dic.TryGetValue(mutualKey2, out var rSeg2))
            {
                Neighbors.Add(rSeg2.Id);
                Edges.Add(default);
            }
        }
    }
}