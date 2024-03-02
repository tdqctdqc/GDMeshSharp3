
using System.Collections.Generic;
using System.Linq;
using Godot;

public class CellGeometry
{
    public Vector2I RelTo { get; private set; }
    public Vector2[] PointsAbs { get; private set; }
    public List<int> Neighbors { get; private set; }
    public List<(Vector2, Vector2)> EdgesRel { get; private set; }

    public CellGeometry(Vector2I relTo, Vector2[] pointsAbs,
        List<int> neighbors, List<(Vector2, Vector2)> edgesRel)
    {
        RelTo = relTo;
        PointsAbs = pointsAbs;
        Neighbors = neighbors;
        EdgesRel = edgesRel;
    }
    public void MakePointsAbs(Vector2I dim)
    {
        var res = new HashSet<Vector2>();
        foreach (var (p1, p2) in EdgesRel)
        {
            var abs1 = p1 + RelTo;
            abs1 = ((Vector2I)abs1).ClampPosition(dim);
            var abs2 = p2 + RelTo;
            abs2 = abs2.ClampPosition(dim);
            res.Add(abs1);
            res.Add(abs2);
        }
        
        PointsAbs = res.ToArray();
    }
}