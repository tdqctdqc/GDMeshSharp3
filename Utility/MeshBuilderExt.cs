
using System.Linq;
using Godot;

public static class MeshBuilderExt
{
    public static void DrawFrontSegment(this MeshBuilder mb,
        Vector2 relTo,
        FrontSegmentAssignment seg, 
        Data d)
    {
        if (seg.LineWaypointIds.Count == 0) return;
        Vector2 relPos(Vector2 p)
        {
            return relTo.GetOffsetTo(p, d);
        }
        for (var i = 0; i < seg.LineWaypointIds.Count - 1; i++)
        {
            var from = MilitaryDomain.GetTacWaypoint(seg.LineWaypointIds[i], d);
            var to = MilitaryDomain.GetTacWaypoint(seg.LineWaypointIds[i + 1], d);
            mb.AddLine(relPos(from.Pos), relPos(to.Pos), Colors.Red, 3f);
        }
        var groups = seg.Groups(d);
        foreach (var group in groups)
        {
            foreach (var unit in group.Units.Items(d))
            {
                var pos = unit.Position.Pos;
                mb.AddPoint(relPos(pos), 10f, Colors.Red);
            }
        }

        var rear = seg.GetRear(d, 3);
        for (var i = 0; i < rear.Count; i++)
        {
            var ring = rear[i];
            foreach (var rWp in ring)
            {
                mb.AddPoint(relPos(rWp.Pos), 10f, ColorsExt.GetRainbowColor(i));
            }
        }
        mb.AddPoint(relPos(MilitaryDomain.GetTacWaypoint(seg.RallyWaypointId, d).Pos),
            20f, Colors.Blue);
    }
    public static void DrawPolyBorders(this MeshBuilder mb,
        Vector2 relTo, MapPolygon poly, Data data)
    {
        var edgeBorders = poly
            .GetOrderedBoundarySegs(data)
            .Select(s => s.Translate(relTo.GetOffsetTo(poly.Center, data)));
        mb.AddLines(edgeBorders.ToList(), 2f, Colors.Black);
    }
}