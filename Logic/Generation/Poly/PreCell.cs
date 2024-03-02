using System;
using System.Collections.Generic;
using System.Linq;
using Godot;

namespace VoronoiSandbox;

public class PreCell : IIdentifiable
{
    public int Id { get; private set; }
    public PrePoly PrePoly { get; set; }
    public Vector2I RelTo => Geometry.RelTo;
    public Vector2[] PointsAbs => Geometry.PointsAbs;
    public List<PreCell> Neighbors { get; private set; }
    public List<(Vector2, Vector2)> EdgesRel => Geometry.EdgesRel;
    public CellGeometry Geometry { get; private set; }
    
    public PreCell(int id, Vector2I relTo)
    {
        Id = id;
        Neighbors = new List<PreCell>();
        Geometry = new CellGeometry(relTo, null,
            new List<int>(), 
            new List<(Vector2, Vector2)>());
    }

    public void AddNeighborAbs(PreCell n, 
        (Vector2I, Vector2I) edgeAbs,
        Vector2I dim)
    {
        if (Neighbors.Contains(n)) throw new Exception();
        Neighbors.Add(n);
        Geometry.Neighbors.Add(n.Id);
        var edgeRel = (edgeAbs.Item1 - RelTo, edgeAbs.Item2 - RelTo);
        EdgesRel.Add(edgeRel);
    }
    public void AddNeighborRel(PreCell n, 
        (Vector2, Vector2) edgeRel)
    {
        Neighbors.Add(n);
        Geometry.Neighbors.Add(n.Id);
        EdgesRel.Add(edgeRel);
    }
    public void ReplaceNeighbor(PreCell removing, PreCell replacement)
    {
        var index = Neighbors.IndexOf(removing);
        if (index == -1) throw new Exception();
        Neighbors[index] = replacement;
        Geometry.Neighbors[index] = index;
    }
    public void ReplaceEdgeRel(PreCell neighbor, 
        (Vector2, Vector2) newEdge)
    {
        var index = Neighbors.IndexOf(neighbor);
        if (index == -1) throw new Exception();
        EdgesRel[index] = newEdge;
    }
    public (Vector2, Vector2) EdgeWith(PreCell n)
    {
        var index = Neighbors.IndexOf(n);
        if (index == -1) throw new Exception();
        return EdgesRel[index];
    }

    public void MakePointsAbs(Vector2I dim)
    {
        Geometry.MakePointsAbs(dim);
    }
}