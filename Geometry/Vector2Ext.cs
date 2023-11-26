using System;
using System.Collections.Generic;
using System.Linq;
using Godot;
using GeometRi;

public static class Vector2Ext
{
    public static bool HasNaN(this Vector2 v)
    {
        return float.IsNaN(v.X) || float.IsNaN(v.Y);
    }
    public static Vector2 RoundTo2Digits(this Vector2 v)
    {
        return new Vector2(v.X.RoundTo2Digits(), v.Y.RoundTo2Digits());
    }
    public static List<Vector2> GeneratePointsAlong(this LineSegment seg, float spacing, 
        float variation, List<Vector2> addTo = null)
    {
        if (addTo == null) addTo = new List<Vector2>();
        var to = seg.To;
        var from = seg.From;
        if (variation >= spacing / 2f) throw new Exception();
        var numPoints = Mathf.FloorToInt((to - from).Length() / spacing );
        var axis = (to - from).Normalized();
        for (int i = 1; i <= numPoints; i++)
        {
            var rand = Game.I.Random.RandfRange(-1f, 1f);
            var p = from + axis * (spacing * i);
            addTo.Add(p);
        }

        return addTo;
    }
    
    public static List<Vector2> GeneratePointsAlong(this Vector2 to, float spacing, float variation, bool includeEndPoints,
            List<Vector2> addTo = null, Vector2 from = default)
    {
        if (variation >= spacing / 2f) throw new Exception();
        if (addTo == null) addTo = new List<Vector2>();

        
        if(includeEndPoints) addTo.Add(from);
        var numPoints = Mathf.FloorToInt((to - from).Length() / spacing ) - 1;
        var axis = (to - from).Normalized();
        for (int i = 1; i <= numPoints; i++)
        {
            var rand = Game.I.Random.RandfRange(-1f, 1f);
            var p = from + axis * (spacing * i);
            addTo.Add(p);
        }
        if(includeEndPoints) addTo.Add(to);

        return addTo;
    }
    public static Vector2 Intify(this Vector2 v)
    {
        return new Vector2(Mathf.FloorToInt(v.X), Mathf.FloorToInt(v.Y));
    }
    public static Vector2 Avg(this List<Vector2> points)
    {
        var res = Vector2.Zero;
        points.ForEach(p => res += p);
        return res / points.Count;
    }
    public static Vector2 Avg(this IEnumerable<Vector2> v)
    {
        return Sum(v) / v.Count();
    }
    public static Vector2 Sum(this IEnumerable<Vector2> v)
    {
        var r = Vector2.Zero;
        foreach (var vector2 in v)
        {
            r += vector2;
        }

        return r;
    }

    public static bool PointIsOnLine(this Vector2 point, Vector2 from, Vector2 to)
    {
        return from.Cross(to) == 0;
    }
    public static bool PointIsInLineSegment(this Vector2 point, Vector2 from, Vector2 to)
    {
        if (point.PointIsOnLine(from, to) == false) return false;
        return point.X >= Mathf.Min(from.X, to.X)
                && point.X <= Mathf.Max(from.X, to.X)
                && point.Y >= Mathf.Min(from.Y, to.Y)
                && point.Y <= Mathf.Max(from.Y, to.Y);
    }

    public static bool LineSegmentsIntersectInclusive(Vector2 p1, Vector2 p2, Vector2 q1, Vector2 q2)
    {
        if (GetLineIntersection(p1, p2, q1, q2, out var intersect))
        {
            var maxPX = Mathf.Max(p1.X, p2.X);
            var minPX = Mathf.Min(p1.X, p2.X);
            var maxQX = Mathf.Max(q1.X, q2.X);
            var minQX = Mathf.Min(q1.X, q2.X);
            return intersect.X <= maxPX && intersect.X >= minPX
                                        && intersect.X <= maxQX && intersect.X >= minQX;
        }

        return false;
    }
    
    public static bool LineSegmentsIntersectExclusive(Vector2 p1, Vector2 p2, Vector2 q1, Vector2 q2)
    {
        if (GetLineIntersection(p1, p2, q1, q2, out var intersect))
        {
            var maxPX = Mathf.Max(p1.X, p2.X);
            var minPX = Mathf.Min(p1.X, p2.X);
            var maxQX = Mathf.Max(q1.X, q2.X);
            var minQX = Mathf.Min(q1.X, q2.X);
            return intersect.X < maxPX && intersect.X > minPX
                                        && intersect.X < maxQX && intersect.X > minQX;
        }

        return false;
    }
    
    
    private static bool OnSegment(Vector2 p, Vector2 q, Vector2 r) 
    { 
        if (q.X <= Mathf.Max(p.X, r.X) && q.X >= Mathf.Min(p.X, r.X) && 
            q.Y <= Mathf.Max(p.Y, r.Y) && q.Y >= Mathf.Min(p.Y, r.Y)) 
            return true; 
  
        return false; 
    } 
  
// To find orientation of ordered triplet (p, q, r). 
// The function returns following values 
// 0 --> p, q and r are collinear 
// 1 --> Clockwise 
// 2 --> Counterclockwise 
    private static int Orientation(Vector2 p, Vector2 q, Vector2 r) 
    { 
        // See https://www.geeksforgeeks.org/orientation-3-ordered-points/ 
        // for details of below formula. 
        float val = (q.Y - p.Y) * (r.X - q.X) - 
                  (q.X - p.X) * (r.Y - q.Y); 
  
        if (val == 0) return 0;  // collinear 
  
        return (val > 0)? 1: 2; // clock or counterclock wise 
    } 
  
// The main function that returns true if line segment 'p1q1' 
// and 'p2q2' intersect. 
    public static bool LineSegIntersect(Vector2 p0, Vector2 p1, 
        Vector2 q0, Vector2 q1, bool inclusive)
    {
        if (inclusive == false)
        {
            if (p0 == q0 || p0 == q1 || p1 == q0 || p1 == q1)
            {
                return false;
            }
        }
        return intersect(p0, p1, q0, q1);
    }
    private static bool intersect(Vector2 p1, Vector2 q1, 
        Vector2 p2, Vector2 q2) 
    { 
        // Find the four orientations needed for general and 
        // special cases 
        int o1 = Orientation(p1, q1, p2); 
        int o2 = Orientation(p1, q1, q2); 
        int o3 = Orientation(p2, q2, p1); 
        int o4 = Orientation(p2, q2, q1); 
  
        // General case 
        if (o1 != o2 && o3 != o4) 
            return true;
  
        // Special Cases 
        // p1, q1 and p2 are collinear and p2 lies on segment p1q1 
        if (o1 == 0 && OnSegment(p1, p2, q1)) return true; 
  
        // p1, q1 and q2 are collinear and q2 lies on segment p1q1 
        if (o2 == 0 && OnSegment(p1, q2, q1)) return true; 
  
        // p2, q2 and p1 are collinear and p1 lies on segment p2q2 
        if (o3 == 0 && OnSegment(p2, p1, q2)) return true; 
  
        // p2, q2 and q1 are collinear and q1 lies on segment p2q2 
        if (o4 == 0 && OnSegment(p2, q1, q2)) return true; 
  
        return false; // Doesn't fall in any of the above cases 
    }
    
    public static bool GetLineIntersection(Vector2 p1, Vector2 p2, Vector2 q1, Vector2 q2, out Vector2 intersect)
    {
        if (p1.X == p2.X)
        {
            if (q1.X == q2.X)
            {
                //either same line or no intersection
                intersect = Vector2.Inf;
                return false;
            }

            return GetIntersectionForVertical(p1.X, q1, q2, out intersect);
        }
        if (q1.X == q2.X)
        {
            return GetIntersectionForVertical(q1.X, p1, p2, out intersect);
        }
    
        var slopeIntercept1 = GetLineSlopeAndIntercept(p1, p2);
        var slopeIntercept2 = GetLineSlopeAndIntercept(q1, q2);
        var determ = (slopeIntercept1.X * -1f) - (slopeIntercept2.X * -1f);

        if (determ == 0f)
        {
            intersect = new Vector2(Single.NaN, Single.NaN);
            return false;
        }
        var x = (slopeIntercept1.Y - slopeIntercept2.Y) / determ;
        var y = (slopeIntercept1.X * -slopeIntercept2.Y -  slopeIntercept2.X * -slopeIntercept1.Y) / determ;
        intersect = new Vector2(x, y);

        return true;
    }

    private static bool GetIntersectionForVertical(float x, Vector2 p1, Vector2 p2, out Vector2 intersect)
    {
        var left = Mathf.Min(p1.X, p2.X);
        var right = Mathf.Max(p1.X, p2.X);
        var bottom = Mathf.Min(p1.Y, p2.Y);
        var top = Mathf.Max(p1.Y, p2.Y);

        var dist = Mathf.Abs(right - left);
        var ratio = (x - left) / dist;
        intersect = new Vector2(x, bottom + Mathf.Abs(top - bottom) * ratio);
        return true;
    }
    public static Vector2? GetLineSegmentsIntersection(Vector2 p1, Vector2 p2, Vector2 q1, Vector2 q2)
    {
        var slopeIntercept1 = GetLineSlopeAndIntercept(p1, p2);
        var slopeIntercept2 = GetLineSlopeAndIntercept(q1, q2);
        var determ = (slopeIntercept1.X * -1f) - (slopeIntercept2.X * -1f);

        if (determ == 0f) return null;
        var x = (slopeIntercept1.Y - slopeIntercept2.Y) / determ;
        var y = (slopeIntercept1.X * -slopeIntercept2.Y -  slopeIntercept2.X * -slopeIntercept1.Y) / determ;
        var point = new Vector2(x, y);
        if (PointIsInLineSegment(point, p1, p2)
            && PointIsInLineSegment(point, q1, q2))
        {
            return point;
        }
        
        return null;
    }
    
    public static Vector2 GetLineSlopeAndIntercept(Vector2 p1, Vector2 p2)
    {
        var left = p1.X < p2.X
            ? p1
            : p2;
        var right = p1.X < p2.X
            ? p2
            : p1;
        var slope = (right.Y - left.Y) / (right.X - left.X);

        var intercept = p1.Y - slope * p1.X;
        return new Vector2(slope, intercept);
    }
    public static float GetProjectionLength(this Vector2 v, Vector2 onto)
    {
        var angle = v.AngleTo(onto);
        return v.Dot(onto) / onto.Length();
    }

    public static Vector2 ClampToBox(this Vector2 p, Vector2 bound1, Vector2 bound2)
    {
        var minXBound = Mathf.Min(bound1.X, bound2.X);
        var minYBound = Mathf.Min(bound1.Y, bound2.Y);
        var maxXBound = Mathf.Max(bound1.X, bound2.X);
        var maxYBound = Mathf.Max(bound1.Y, bound2.Y);

        return new Vector2(Mathf.Clamp(p.X, minXBound, maxXBound),
            Mathf.Clamp(p.Y, minYBound, maxYBound));
    }
    public static float DistToLine(this Vector2 point, Vector2 start, Vector2 end)
    {
        var theta = Mathf.Abs((point - start).AngleTo(end - start));
        return Mathf.Sin(theta) * point.DistanceTo(start);
        
        
        
        
        // vector AB
        var AB = new Vector2();
        AB.X = end.X - start.X;
        AB.Y = end.Y - start.Y;
 
        // vector BP
        var BE = new Vector2();
        BE.X = point.X - end.X;
        BE.Y = point.Y - end.Y;
 
        // vector AP
        var AE = new Vector2();
        AE.X = point.X - start.X;
        AE.Y = point.Y - start.Y;
 
        // Variables to store dot product
        float AB_BE, AB_AE;
 
        // Calculating the dot product
        AB_BE = (AB.X * BE.X + AB.Y * BE.Y);
        AB_AE = (AB.X * AE.X + AB.Y * AE.Y);
 
        // Minimum distance from
        // point E to the line segment
        float reqAns = 0;
 
        // Case 1
        if (AB_BE > 0)
        {
 
            // Finding the magnitude
            var y = point.Y - end.Y;
            var x = point.X - end.X;
            reqAns = Mathf.Sqrt(x * x + y * y);
        }
 
        // Case 2
        else if (AB_AE < 0)
        {
            var y = point.Y - start.Y;
            var x = point.X - start.X;
            reqAns = Mathf.Sqrt(x * x + y * y);
        }
 
        // Case 3
        else
        {
 
            // Finding the perpendicular distance
            var x1 = AB.X;
            var y1 = AB.Y;
            var x2 = AE.X;
            var y2 = AE.Y;
            var mod = Mathf.Sqrt(x1 * x1 + y1 * y1);
            reqAns = Mathf.Abs(x1 * y2 - y1 * x2) / mod;
        }
        return reqAns;
    }
    public static Vector2 GetPointAlongLine(this IList<Vector2> points,
        Func<Vector2, Vector2, Vector2> getOffset,
        float ratio)
    {
        if (ratio < 0f || ratio > 1f)
        {
            throw new Exception("ratio is " + ratio);
        }
        var totalLength = 0f;
        for (var i = 0; i < points.Count - 1; i++)
        {
            totalLength += getOffset(points[i], points[i + 1]).Length();
        }

        var targetLength = ratio * totalLength;
        var soFar = 0f;
        for (var i = 0; i < points.Count - 1; i++)
        {
            var from = points[i];
            var to = points[i + 1];
            var offset = getOffset(from, to);
            var toGo = targetLength - soFar;
            if (toGo > offset.Length())
            {
                soFar += offset.Length();
                continue;
            }

            return from + offset.Normalized() * toGo;
        }

        return points.Last();
    }
    
    public static Vector2 GetPointAlongCircle(this IList<Vector2> points,
        Func<Vector2, Vector2, Vector2> getOffset,
        float ratio)
    {
        if (ratio < 0f || ratio > 1f) throw new Exception();
        var totalLength = 0f;
        for (var i = 0; i < points.Count; i++)
        {
            totalLength += getOffset(points[i], points.Modulo(i + 1)).Length();
        }

        var targetLength = ratio * totalLength;
        var soFar = 0f;
        for (var i = 0; i <= points.Count; i++)
        {
            var from = points.Modulo(i);
            var to = points.Modulo(i + 1);
            var offset = getOffset(from, to);
            var toGo = targetLength - soFar;
            if (toGo > offset.Length())
            {
                soFar += offset.Length();
                continue;
            }

            return from + offset.Normalized() * toGo;
        }

        throw new Exception();
    }

    public static void LinkRings<T>(this IList<T> outerRing, 
        IList<T> innerRing, Vector2 center, Func<T, Vector2> getPos,
        Action<T, T> link)
    {
        foreach (var inner in innerRing)
        {
            foreach (var outer in outerRing)
            {
                link(inner, outer);
            }
        }

        return;

        for (var i = 0; i < innerRing.Count; i++)
        {
            var inner = innerRing[i];
            var axis = getPos(inner) - center;
            var left = innerRing.Where(t => t.Equals(inner) == false)
                .MinBy(t => axis.GetCCWAngleTo(getPos(t) - center));
            var leftAngle = axis.GetCCWAngleTo(getPos(left) - center);
            var right = innerRing.Where(t => t.Equals(inner) == false)
                .MinBy(t => axis.GetClockwiseAngleTo(getPos(t) - center));
            var rightAngle = axis.GetClockwiseAngleTo(getPos(right) - center);
            var toLink = outerRing
                .Where(t =>
                {
                    return axis.GetCCWAngleTo(getPos(t) - center) <= leftAngle
                        ||
                        axis.GetClockwiseAngleTo(getPos(t) - center) <= rightAngle;
                });
            foreach (var t in toLink)
            {
                link(inner, t);
            }
        }
    }
}
