
using System.Collections.Generic;
using System.Linq;
using Godot;

public class CellGeometry
{
    public Vector2I RelTo { get; private set; }
    public Vector2[] PointsRel { get; private set; }
    public List<int> Neighbors { get; private set; }
    public List<(Vector2, Vector2)> EdgesRel { get; private set; }

    public CellGeometry(Vector2I relTo, Vector2[] pointsRel,
        List<int> neighbors, List<(Vector2, Vector2)> edgesRel)
    {
        RelTo = relTo;
        PointsRel = pointsRel;
        Neighbors = neighbors;
        EdgesRel = edgesRel;
    }
    public void MakePointsRel(Vector2I dim)
    {
        var start = (Vector2)EdgesRel[0].Item1;
        var res = GetEdgePoints().Distinct()
            .OrderBy(p => start.GetCWAngleTo(p));
        PointsRel = res.ToArray();
    }

    private IEnumerable<Vector2> GetEdgePoints()
    {
        for (var i = 0; i < EdgesRel.Count; i++)
        {
            var e = EdgesRel[i];
            yield return e.Item1;
            yield return e.Item2;
        }
    }
}