using System;
using System.Collections.Generic;
using System.Linq;
using Godot;
using LightObjectPool;

public class MeshBuilder
{
    private static Pool<MeshBuilder> _pool;
    public List<Vector2> TriVertices { get; private set; }
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
        TriVertices = new List<Vector2>();
        Colors = new List<Color>();
        Labels = new List<Label>();
    }
    public void Return()
    {
        _pool.Return(this);
    }
    public void Clear()
    {
        TriVertices.Clear();
        Colors.Clear();
    }

   
    public void AddTri(Vector2 a, Vector2 b, Vector2 c, Color color)
    {
        TriVertices.Add(a);
        TriVertices.Add(b);
        TriVertices.Add(c);
        Colors.Add(color);
        Colors.Add(color);
        Colors.Add(color);
    }
    
    public void AddTriRel(Vector2 a, Vector2 b, Vector2 c, Color color,
        Vector2 relTo, Data d)
    {
        TriVertices.Add(relTo.Offset(a, d));
        TriVertices.Add(relTo.Offset(b, d));
        TriVertices.Add(relTo.Offset(c, d));
        Colors.Add(color);
        Colors.Add(color);
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
        Func<MapPolygon, Color> getColor,
        float thickness, Vector2 relTo, Data d)
    {
        var offset = relTo.Offset(poly.Center, d);
        var edge = poly.GetEdge(n, d);
        
        var edges = poly.GetCells(d)
            .OfType<LandCell>()
            .SelectMany(c => c.GetNeighbors(d)
                .OfType<LandCell>()
                .Where(n => n.Polygon.RefId == n.Id)
                .Select(n => (c, n))
            );
        var color = getColor(poly);
        foreach (var v in edges)
        {
            DrawPolyCellEdge(v.c, v.n, 
                c => color, 
                thickness, relTo, d);
        }
    }
    public void DrawPolyCellEdge(Cell c, Cell n,
        Func<Cell, Color> getColor,
        float thickness, Vector2 relTo, Data d,
        bool debug = false)
    {
        var color = getColor(c);
        var edge = c.GetEdgeRelWith(n);
        var offsetToCenter = relTo.Offset(c.GetCenter(), d);
        var perp = Clockwise
            .GetPerpTowards(edge.Item1, edge.Item2, 
                Vector2.Zero).Normalized() * thickness;
        var innerSeg = (edge.Item1 + perp, edge.Item2 + perp);
        var mid = (edge.Item1 + edge.Item2) / 2f;
        var innerMid = mid + perp;
        var mutuals = c.Neighbors.Intersect(n.Neighbors)
            .Select(i => PlanetDomainExt.GetPolyCell(i, d)).ToArray();
        // if (mutuals.Length > 2) throw new Exception();
        
        for (var i = 0; i < mutuals.Length; i++)
        {
            var mutual = mutuals[i];
            var mEdge = c.GetEdgeRelWith(mutual);
            if (mEdge == default) continue;
            var shared = getShared(edge, mEdge);
            if (Vector2Ext.LineSegIntersect(innerSeg.Item1, 
                    innerSeg.Item2,
                    mEdge.Item1, mEdge.Item2, true,
                    out var intersectPoint))
            {
                //acute
                AddTriRel(mid + c.RelTo, 
                    innerMid + c.RelTo, intersectPoint + c.RelTo, color,
                    relTo, d);
                AddTriRel(mid + c.RelTo, shared + c.RelTo, 
                    intersectPoint + c.RelTo, color,
                    relTo, d);
                
            }
            else
            {
                var axis = mid - shared;
                var mExclusive = getExclusive(mEdge, shared);
                var mAxis = mExclusive - shared;
                var mLength = Mathf.Min(thickness, mEdge.Item1.DistanceTo(mEdge.Item2));
                var mPoint = shared + mAxis.Normalized() * mLength;
                AddTriRel(mid + c.RelTo, shared + c.RelTo, 
                    shared + perp + c.RelTo, color,
                    relTo, d);
                AddTriRel(innerMid + c.RelTo, mid + c.RelTo, 
                    shared + perp + c.RelTo, color,
                    relTo, d);
                AddTriRel(shared + perp + c.RelTo, 
                    shared + c.RelTo, mPoint + c.RelTo, color, 
                     relTo, d);
            }
            
            
            if (mutuals.Length == 1)
            {
                var exclusive = getExclusive(edge, shared);
                AddTriRel(innerMid + c.RelTo, mid + c.RelTo, 
                    exclusive + perp + c.RelTo, color,
                    relTo, d);
                AddTriRel(exclusive + c.RelTo, mid + c.RelTo, 
                    exclusive + perp + c.RelTo, color,
                    relTo, d);
            }
        }

        

        Vector2 getShared((Vector2, Vector2) e1, (Vector2, Vector2) e2)
        {
            if (e1.Item1 == e2.Item1 || e1.Item1 == e2.Item2)
            {
                return e1.Item1;
            }

            if (e1.Item2 == e2.Item1 || e1.Item2 == e2.Item2)
            {
                return e1.Item2;
            }

            throw new Exception();
        }

        Vector2 getExclusive((Vector2, Vector2) e, Vector2 shared)
        {
            if (e.Item1 != shared) return e.Item1;
            if (e.Item2 != shared) return e.Item2;
            throw new Exception();
        }
    }

    public void AddLine(Vector2 from, Vector2 to, Color color, float thickness)
    {
        JoinLinePoints(from, to, thickness, color);
    }
    public void AddLineRel(Vector2 from, Vector2 to, Color color,
        float thickness, Vector2 relTo, Data d)
    {
        JoinLinePoints(relTo.Offset(from, d), relTo.Offset(to, d), thickness, color);
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
    
    public void AddArrowRel(Vector2 from, Vector2 to, 
        float thickness, Color color, Vector2 relTo, Data d)
    {
        var length = from.DistanceTo(to);
        var arrowLength = Mathf.Min(length / 2f, thickness * 1.5f);
        var stemLength = length - arrowLength;

        var axis = (to - from).Normalized();
        var orth = axis.Orthogonal();

        var arrowBase = relTo.Offset(from + axis * stemLength, d);
        var relT = relTo.Offset(to, d);
        var relF = relTo.Offset(from, d);
        AddTri(relT, arrowBase + orth * thickness,
            arrowBase - orth * thickness, color);
        AddLine(relF, arrowBase, color, thickness);
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
        if (TriVertices.Count == 0) return new MeshInstance2D();
        var mesh = MeshGenerator.GetArrayMesh(
            TriVertices.ToArray(), 
            Colors.ToArray());
        var meshInstance = new MeshInstance2D();
        meshInstance.Mesh = mesh;
        Labels.ForEach(l => meshInstance.AddChild(l));
        return meshInstance;
    }
}