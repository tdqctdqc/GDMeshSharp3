using Godot;
using System;
using System.Collections.Generic;
using System.Linq;

public static class GeometryExt
{
    public static bool InBox(this Vector2 point, Vector2 boxPos, Vector2 boxDim)
    {
        return point.X >= boxPos.X && point.X <= boxPos.X + boxDim.X
         && point.Y >= boxPos.Y && point.Y <= boxPos.Y + boxDim.Y;
    }

    public static float RadToDegrees(this float rad)
    {
        return (360f * rad / (Mathf.Pi * 2f));
    }
    public static float DegreesToRad(this float degrees)
    {
        return Mathf.Pi * 2f * degrees / 360f;
    }
    public static bool PointIsOnLineSegment(this Vector2 point, Vector2 seg1, Vector2 seg2)
    {
        var axis = (seg1 - seg2).Normalized();
        var ray = (seg1 - point).Normalized();
        if (axis != ray && axis != -ray) return false;
        return ((seg1.X <= point.X && point.X <= seg2.X) || (seg2.X <= point.X && point.X <= seg1.X))
               &&
               ((seg1.Y <= point.Y && point.Y <= seg2.Y) || (seg2.Y <= point.Y && point.Y <= seg1.Y));
    }

    public static Vector2[] UnifyPolygons(this Vector2[] poly1, Vector2[] poly2)
    {
        var unions = Geometry2D.MergePolygons(poly1, poly2);
        if (unions.Count != 1) throw new Exception();
        return unions[0];
    }
    public static bool TryUnifyPolygons(this Vector2[] poly1, 
        Vector2[] poly2, out Vector2[] newBounds)
    {
        newBounds = null;
        var unions = Geometry2D.MergePolygons(poly1, poly2);
        if (unions.Count != 1) return false;
        newBounds = unions[0];
        
        return true;
    }

    public static Vector2[] ClipPolygons(this Vector2[] polygon,
        Vector2[] clip)
    {
        var differences = Geometry2D.ClipPolygons(polygon, clip);
        if (differences.Count != 1) throw new Exception();
        return differences[0];
    }
    public static bool TryClipPolygons(this Vector2[] polygon,
        Vector2[] clip, out Vector2[] newBounds)
    {
        newBounds = null;
        var differences = Geometry2D.ClipPolygons(polygon, clip);
        if (differences.Count != 1) return false;
        newBounds = differences[0];
        return true;
    }

    public static Vector2[] IntersectPolygons(this Vector2[] polygon,
        Vector2[] clip)
    {
        var intersects = Geometry2D.IntersectPolygons(polygon, clip);
        if (intersects.Count != 1) throw new Exception();
        return intersects[0];
    }
    
    public static bool TryIntersectPolygons(this Vector2[] polygon,
        Vector2[] intersect, out Vector2[] newBounds)
    {
        newBounds = null;
        var intersects = Geometry2D.IntersectPolygons(polygon, intersect);
        if (intersects.Count != 1) return false;
        newBounds = intersects[0];
        return true;
    }

    public static Vector2[] GetBiggestClip(this Vector2[] polygon,
        Vector2[] clip)
    {
        var differences = Geometry2D.ClipPolygons(polygon, clip);
        if (differences.Count == 0) throw new Exception();
        if (differences[0].Length == 0) throw new Exception();
        return differences.MaxBy(p => p.GetArea());
    }

    public static Vector2[] ShrinkGrowPoly(this Vector2[] polygon,
        float growBy)
    {
        var res = Geometry2D.OffsetPolygon(polygon, growBy);
        if (res.Count != 1) throw new Exception();
        return res[0];
    }
    public static bool TryShrinkGrowPoly(this Vector2[] polygon,
        float growBy, out Vector2[] newPoly)
    {
        newPoly = null;
        var res = Geometry2D.OffsetPolygon(polygon, growBy);
        if (res.Count != 1) return false;
        newPoly = res[0];
        return true;
    }

    public static List<Vector2[]> GetCellUnionPolygons(
        IEnumerable<Cell> cells, 
        Vector2 relTo,
        Data d)
    {
        var res = new List<Vector2[]>();
        var hash = cells.ToHashSet();
        while (hash.Count > 0)
        {
            var queue = new Queue<Cell>();
            var first = hash.First();
            hash.Remove(first);
            queue.Enqueue(first);
            Vector2[] boundary = null;
            while (queue.Count > 0)
            {
                var c = queue.Dequeue();
                if (boundary is null)
                {
                    boundary = getRelRelBoundary(c);
                }
                else
                {
                    boundary = GeometryExt.UnifyPolygons(boundary,
                        getRelRelBoundary(c));
                }

                foreach (var neighbor in c.GetNeighbors(d))
                {
                    if (hash.Contains(neighbor) == false) continue;
                    if (neighbor is RiverCell) continue;
                    hash.Remove(neighbor);
                    queue.Enqueue(neighbor);
                }
            }
            res.Add(boundary);
        }
        
        Vector2[] getRelRelBoundary(Cell c)
        {
            var offset = relTo.Offset(c.RelTo, d);
            return c.RelBoundary.Select(v => v + offset).ToArray();
        }

        return res;
    }
    
}
