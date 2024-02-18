using System;
using System.Collections.Generic;
using System.Linq;
using Godot;
using LightObjectPool;

public class MeshBuilder
{
    private static Pool<MeshBuilder> _pool;
    public List<Triangle> Tris { get; private set; }
    public List<Color> Colors { get; private set; }
    public List<Label> Labels { get; private set; }
    static MeshBuilder()
    {
        var policy = new PoolPolicy<MeshBuilder>(
            f => f.GetPooledObject().Value,
            p => p.Clear(),
            1000
        );
        _pool = LightObjectPool.Pool.Create<MeshBuilder>(p => p.Clear(), 100);
    }

    public static MeshBuilder GetFromPool()
    {
        return _pool.Get();
    }

    public MeshBuilder()
    {
        Tris = new List<Triangle>();
        Colors = new List<Color>();
        Labels = new List<Label>();
    }
    public void Return()
    {
        _pool.Return(this);
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
    
    public void DrawPolyCellEdge(PolyCell c1, PolyCell c2, 
        Func<PolyCell, Color> color,
        float thickness, Vector2 relTo, Data d)
    {

        return;
        Vector2? lineP1 = null;
        Vector2? lineP2 = null;
        for (var i = 0; i < c1.RelBoundary.Length; i++)
        {
            var from1 = c1.RelBoundary[i];
            var to1 = c1.RelBoundary.Modulo(i + 1);
            for (var j = 0; j < c2.RelBoundary.Length; j++)
            {
                var from2 = c2.RelBoundary[j];
                var from2rel = c1.RelTo.GetOffsetTo(from2 + c2.RelTo, d);
                var to2 = c2.RelBoundary.Modulo(j + 1);
                var to2rel = c1.RelTo.GetOffsetTo(to2 + c2.RelTo, d);
                var close1 = Geometry2D
                    .GetClosestPointToSegment(from1, from2rel, to2rel);
                var dist1 = close1.DistanceTo(from1);
                
                var close2 = Geometry2D
                    .GetClosestPointToSegment(from2rel, from1, to1);
                var dist2 = close2.DistanceTo(from2rel);
                
                if ((dist1 < .1f || from1.DistanceTo(from2rel) < .1f || from1.DistanceTo(to2rel) < .1f)
                    && alreadyFound(from1) == false)
                {
                    register(from1);
                }

                if (foundBoth()) break;
                if ((dist2 < .1f || from2rel.DistanceTo(from1) < .1f || from2rel.DistanceTo(to1) < .1f)
                    && alreadyFound(from2rel) == false)
                {
                    register(from2rel);
                }
                if (foundBoth()) break;
            }
            if (foundBoth()) break;
        }

        if (foundBoth() == false)
        {
            return;
        }
        var rel1 = relTo.GetOffsetTo(lineP1.Value + c1.RelTo, d);
        var rel2 = relTo.GetOffsetTo(lineP2.Value + c1.RelTo, d);
        AddLine(rel1, rel2, color(c1), thickness);
        
        void register(Vector2 p)
        {
            if (foundBoth()) throw new Exception();
            if (lineP1 == null)
            {
                lineP1 = p;
            }
            else if (lineP2 == null)
            {
                lineP2 = p;
            }
        }

        bool foundBoth()
        {
            return lineP1 is not null && lineP2 is not null;
        }

        bool alreadyFound(Vector2 p)
        {
            return lineP1 == p || lineP2 == p;
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
        var arrowLength = Mathf.Min(length / 2f, thickness * 1.5f);
        var stemLength = length - arrowLength;

        var axis = (to - from).Normalized();
        var orth = axis.Orthogonal();

        var arrowBase = from + axis * stemLength;
        
        AddTri(to, arrowBase + orth * thickness,
            arrowBase - orth * thickness, color);
        AddLine(from, arrowBase, color, thickness);
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