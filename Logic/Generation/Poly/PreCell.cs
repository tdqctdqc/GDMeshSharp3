using System;
using System.Collections.Generic;
using Godot;


public class PreCell : IIdentifiable
{
    public int Id { get; private set; }
    public PrePoly PrePoly { get; set; }
    public Vector2 RelTo { get; private set; }
    public List<PreCell> Neighbors { get; private set; }
    public List<(Vector2, Vector2)> EdgesRel { get; private set; }
    
    public PreCell(GenWriteKey key, Vector2 relTo)
    {
        Id = key.Data.IdDispenser.TakeId();
        RelTo = relTo;
        Neighbors = new List<PreCell>();
        EdgesRel = new List<(Vector2, Vector2)>();
    }

    public void AddNeighborAbs(PreCell n, (Vector2, Vector2) edgeAbs,
        Vector2I dim)
    {
        // if (Neighbors.Contains(n)) throw new Exception();
        Neighbors.Add(n);
        var edgeRel = (edgeAbs.Item1 - RelTo, edgeAbs.Item2 - RelTo);
        EdgesRel.Add(edgeRel);
    }
    public void AddNeighborRel(PreCell n, (Vector2, Vector2) edgeRel)
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
    public void ReplaceEdge(PreCell neighbor, (Vector2, Vector2) newEdge)
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

    public HashSet<Vector2> GetPointsAbs(Vector2I dim, Data d)
    {
        var res = new HashSet<Vector2>();
        foreach (var (p1, p2) in EdgesRel)
        {
            var abs1 = p1 + RelTo;
            abs1 = abs1.ClampPosition(d);
            var abs2 = p2 + RelTo;
            abs2 = abs2.ClampPosition(d);
            res.Add(abs1);
            res.Add(abs2);
        }
        return res;
    }
}