using System;
using System.Collections.Generic;
using System.Linq;
using Godot;

namespace VoronoiSandbox;

public class PreCell : IIdentifiable
{
    public int Id { get; private set; }
    public PrePoly PrePoly { get; set; }
    public Vector2I RelTo { get; private set; }
    public Vector2I[] PointsAbs { get; private set; }
    public List<PreCell> Neighbors { get; private set; }
    public List<(Vector2I, Vector2I)> EdgesRel { get; private set; }
    
    public PreCell(int id, Vector2I relTo)
    {
        Id = id;
        RelTo = relTo;
        Neighbors = new List<PreCell>();
        EdgesRel = new List<(Vector2I, Vector2I)>();
    }

    public void AddNeighborAbs(PreCell n, 
        (Vector2I, Vector2I) edgeAbs,
        Vector2I dim)
    {
        if (Neighbors.Contains(n)) throw new Exception();
        Neighbors.Add(n);
        var edgeRel = (edgeAbs.Item1 - RelTo, edgeAbs.Item2 - RelTo);
        EdgesRel.Add(edgeRel);
    }
    public void AddNeighborRel(PreCell n, 
        (Vector2I, Vector2I) edgeRel)
    {
        Neighbors.Add(n);
        EdgesRel.Add(edgeRel);
    }
    public void ReplaceNeighbor(PreCell removing, PreCell replacement)
    {
        var index = Neighbors.IndexOf(removing);
        if (index == -1) throw new Exception();
        Neighbors[index] = replacement;
    }
    public void ReplaceEdgeRel(PreCell neighbor, 
        (Vector2I, Vector2I) newEdge)
    {
        var index = Neighbors.IndexOf(neighbor);
        if (index == -1) throw new Exception();
        EdgesRel[index] = newEdge;
    }
    public (Vector2I, Vector2I) EdgeWith(PreCell n)
    {
        var index = Neighbors.IndexOf(n);
        if (index == -1) throw new Exception();
        return EdgesRel[index];
    }

    public void MakePointsAbs(Vector2I dim)
    {
        var res = new HashSet<Vector2I>();
        foreach (var (p1, p2) in EdgesRel)
        {
            var abs1 = p1 + RelTo;
            abs1 = abs1.ClampPosition(dim);
            var abs2 = p2 + RelTo;
            abs2 = abs2.ClampPosition(dim);
            res.Add(abs1);
            res.Add(abs2);
        }
        
        PointsAbs = res.ToArray();
    }
}