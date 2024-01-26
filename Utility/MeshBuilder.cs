using System;
using System.Collections.Generic;
using System.Linq;
using Godot;

public class MeshBuilder
{
    public List<Triangle> Tris { get; private set; }
    public List<Color> Colors { get; private set; }
    public List<Label> Labels { get; private set; }

    public MeshBuilder()
    {
        Tris = new List<Triangle>();
        Colors = new List<Color>();
        Labels = new List<Label>();
    }

    public void Clear()
    {
        Tris.Clear();
        Colors.Clear();
    }

    public void AddTriOutline(Triangle tri, float thickness, Color color)
    {
        var center = tri.GetCentroid();
        var aIn = center + (tri.A - center).Normalized() * ((tri.A - center).Length() - thickness);
        var bIn = center + (tri.B - center).Normalized() * ((tri.B - center).Length() - thickness);
        var cIn = center + (tri.C - center).Normalized() * ((tri.C - center).Length() - thickness);
        
        AddTri(tri.A, aIn, tri.B, color);
        AddTri(tri.B, bIn, tri.A, color);
        
        AddTri(tri.A, aIn, tri.C, color);
        AddTri(tri.C, cIn, tri.A, color);
        
        AddTri(tri.B, bIn, tri.C, color);
        AddTri(tri.C, cIn, tri.B, color);
    }
    public void AddTri(Triangle tri, Color color)
    {
        Tris.Add(tri);
        Colors.Add(color);
    }
    
    public void AddTri(Vector2 a, Vector2 b, Vector2 c, Color color)
    {
        var tri = new Triangle(a, b, c);
        Tris.Add(tri);
        Colors.Add(color);
    }
    
    private void JoinLinePoints(Vector2 from, Vector2 to, float thickness, Color color)
    {
        var perpendicular = (from - to).Normalized().Rotated(Mathf.Pi / 2f);
        var fromOut = from + perpendicular * .5f * thickness;
        var fromIn = from - perpendicular * .5f * thickness;
        var toOut = to + perpendicular * .5f * thickness;
        var toIn = to - perpendicular * .5f * thickness;
        AddTri(fromIn, fromOut, toOut, color);
        AddTri(toIn, toOut, fromIn, color);
    }

    public void DrawPolyEdge(MapPolygon poly, MapPolygon n, 
        Func<MapPolygon, Color> color,
        float thickness, MapPolygon relTo, Data d)
    {
        var offset = relTo.GetOffsetTo(poly, d);
        var edge = poly.GetEdge(n, d);
        var segs = edge.GetSegsRel(poly, d).Segments;
        for (var i = 0; i < segs.Count; i++)
        {
            var seg = segs[i];
            var axis = seg.GetNormalizedAxis();
            var perp = axis.Orthogonal() * thickness;
            if (thickness > seg.From.Length()) continue;
            if (thickness > seg.To.Length()) continue;

            var toPerp = seg.To - perp;
            var fromPerp = seg.From - perp;

            AddTri(new Triangle(seg.From, seg.To, toPerp).Transpose(offset), color(poly));
            AddTri(new Triangle(toPerp, seg.From, fromPerp).Transpose(offset), color(poly));

        }
    }

    public void AddLine(Vector2 from, Vector2 to, Color color, float thickness)
    {
        JoinLinePoints(from, to, thickness, color);
    }

    public void AddParallelLines(Vector2 from, Vector2 to, Color color, float thickness, float offset)
    {
        var axis = (to - from).Normalized();
        var perp = axis.Orthogonal();
        var railOffset = offset * perp;
        var railWidth = thickness;
        AddLine(from + railOffset, to + railOffset, color, railWidth);
        AddLine(from - railOffset, to - railOffset, color, railWidth);
    }

    public void AddSpacedCrossbars(Vector2 from, Vector2 to, Color color, float thickness, float length, float spacing)
    {
        var axis = (to - from).Normalized();
        var perp = axis.Orthogonal();
        var numCrossBars = Mathf.FloorToInt(from.DistanceTo(to) / spacing);
        var crossStartOffset = axis * spacing / 2f;
        for (var i = 0; i < numCrossBars; i++)
        {
            var mid = crossStartOffset + axis * i * spacing;
            var left = from + mid - perp * length;
            var right = from + mid + perp * length;
            AddLine(left, right, color, thickness);
        }
    }
    
    public void AddDashedLine(Vector2 from, Vector2 to, Color color, float thickness, float dashLength, float spacing)
    {
        var axis = (to - from).Normalized();
        var perp = axis.Orthogonal();
        var numCrossBars = Mathf.FloorToInt(from.DistanceTo(to) / (spacing + dashLength));
        var startOffset = axis * spacing / 2f;
        for (var i = 0; i < numCrossBars; i++)
        {
            var dashFrom = from + startOffset + axis * i * (spacing + dashLength);
            var dashTo = dashFrom + axis * dashLength;
            AddLine(dashFrom, dashTo, color, thickness);
        }
    }
    public void AddLines(List<Vector2> froms,
        List<Vector2> tos, float thickness, List<Color> colors)
    {
        for (int i = 0; i < froms.Count; i++)
        {
            var color = colors[i];
            JoinLinePoints(froms[i], tos[i], thickness, color);
        }
    }
    public void AddLines(IReadOnlyList<ISegment<Vector2>> segs, float thickness, List<Color> colors)
    {
        for (int i = 0; i < segs.Count; i++)
        {
            var color = colors[i];
            JoinLinePoints(segs[i].From, segs[i].To, thickness, color);
        }
    }
    public void AddLines(IReadOnlyList<ISegment<Vector2>> segs, float thickness, Color color)
    {
        for (int i = 0; i < segs.Count; i++)
        {
            JoinLinePoints(segs[i].From, segs[i].To, thickness, color);
        }
    }
    public void AddLinesCustomWidths(List<Vector2> froms,
        List<Vector2> tos, List<float> widths, List<Color> colors)
    {
        for (int i = 0; i < froms.Count; i++)
        {
            var color = colors[i];
            JoinLinePoints(froms[i], tos[i], widths[i], color);
        }
    }
    
    public void AddCircle(Vector2 center, float radius, int resolution, Color color)
    {
        var angleIncrement = Mathf.Pi * 2f / (float) resolution;
        var triPoints = new List<Vector2>();
        for (int i = 0; i < resolution; i++)
        {
            var startAngle = angleIncrement * i;
            var startPoint = center + Vector2.Up.Rotated(startAngle) * radius;
            var endAngle = startAngle + angleIncrement;
            var endPoint = center + Vector2.Up.Rotated(endAngle) * radius;
            AddTri(center, startPoint, endPoint, color);
        }
    }

    public void AddArrowsRainbow(IReadOnlyList<LineSegment> segs, float thickness)
    {
        for (var i = 0; i < segs.Count; i++)
        {
            AddArrow(segs[i].From, segs[i].To, thickness, ColorsExt.GetRainbowColor(i));
        }
    }
    public void AddArrows(IReadOnlyList<LineSegment> segs, float thickness, Color color)
    {
        foreach (var s in segs)
        {
            AddArrow(s.From, s.To, thickness, color);
        }
    }
    public void AddArrow(Vector2 from, Vector2 to, 
        float thickness, Color color)
    {
        var length = from.DistanceTo(to);
        var arrowSize = Mathf.Min(length / 2f, thickness * 2f);
        var mid = (from + to) / 2f;
        
        var axis = (to - from).Normalized();
        var perpendicular = axis.Rotated(Mathf.Pi / 2f);
        var arrowFrom = mid - axis * arrowSize;
        var arrowTo = mid + axis * arrowSize;
        JoinLinePoints(from, to, thickness, color);
        AddTri(arrowTo, arrowFrom + perpendicular * arrowSize / 2f,
            arrowFrom - perpendicular * arrowSize / 2f, color);
    }

    public void AddNumMarkers(List<Vector2> points, float markerSize, Color color, Color textColor, Vector2 offset,
        string tag = "")
    {
        AddPointMarkers(points, markerSize, color);
        for (var i = 0; i < points.Count; i++)
        {
            var label = new Label();
            label.Text = tag + " " + i.ToString();
            label.Position = points[i] + offset;
            label.SelfModulate = textColor;
            Labels.Add(label);
        }
    }
    public void AddPointMarkers(List<Vector2> points, float markerSize, Color color)
    {
        foreach (var p in points)
        {
            AddPoint(p, markerSize, color);
        }
    }

    public void AddPoint(Vector2 p, float size, Color color)
    {
        var topLeft = p + Vector2.Up * size / 2f
                        + Vector2.Left * size / 2f;
        var topRight = p + Vector2.Up * size / 2f
                         + Vector2.Right * size / 2f;
        var bottomLeft = p + Vector2.Down * size / 2f
                           + Vector2.Left * size / 2f;
        var bottomRight = p + Vector2.Down * size / 2f
                            + Vector2.Right * size / 2f;
        AddTri(topLeft, topRight, bottomLeft, color);
        AddTri(topRight, bottomRight, bottomLeft, color);
    }
    public MeshInstance2D GetMeshInstance()
    {
        if (Tris.Count == 0) return new MeshInstance2D();
        var mesh = MeshGenerator.GetArrayMesh(Tris.GetTriPoints().ToArray(), Colors.ToArray());
        var meshInstance = new MeshInstance2D();
        meshInstance.Mesh = mesh;
        Labels.ForEach(l => meshInstance.AddChild(l));
        return meshInstance;
    }
}