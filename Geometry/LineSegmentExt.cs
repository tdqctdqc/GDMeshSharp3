using System;
using System.Collections.Generic;
using System.Linq;
using Godot;


public static class LineSegmentExt
{

    public static List<LineSegment> FindTri(this List<LineSegment> segs)
    {
        for (var i = 0; i < segs.Count; i++)
        {
            var seg = segs[i];
            var pointsTo = segs.Where(s => seg.To == s.From);
            if (pointsTo.Count() == 0) continue;
            foreach (var cand in pointsTo)
            {
                var complete = segs.FirstOrDefault(s => s.To == seg.From && s.From == cand.To);
                if (complete is LineSegment ls) return new List<LineSegment> {seg, cand, complete};
            }
        }

        return null;
    }

    public static bool IntersectsExclusive(this LineSegment ls, Vector2 a, Vector2 b)
    {
        return Vector2Ext.LineSegmentsIntersectExclusive(ls.From, ls.To, a, b);
    }
    public static bool IntersectsInclusive(this LineSegment ls, Vector2 a, Vector2 b)
    {
        return Vector2Ext.LineSegmentsIntersectInclusive(ls.From, ls.To, a, b);
    }
    public static bool Intersects(this LineSegment ls, Vector2 point, Vector2 dir)
    {
        var intersect = Geometry2D.LineIntersectsLine(ls.From, ls.GetNormalizedAxis(), point, dir);
        if (intersect.Obj is Vector2 v == false) return false;
        var inX = (ls.From.X <= v.X && v.X <= ls.To.X) || (ls.From.X >= v.X && v.X >= ls.To.X);
        var inY = (ls.From.Y <= v.Y && v.Y <= ls.To.Y) || (ls.From.Y >= v.Y && v.Y >= ls.To.Y);
        return inX && inY;
    }
    public static float GetAngleAroundSum(this List<LineSegment> segs, Vector2 center)
    {
        float res = 0f;
        for (var i = 0; i < segs.Count; i++)
        {
            var seg = segs[i];
            res += (seg.From - center).AngleTo(seg.To - center);
        }
        return res;
    }
    public static bool IsSame(this LineSegment ls, LineSegment compare)
    {
        if (ls == null && compare == null) return true;
        if (ls == null || compare == null) return false;
        return ls.From == compare.From && ls.To == compare.To;
    }
    public static Vector2 Average(this IEnumerable<LineSegment> segs)
    {
        var avgX = segs.Average(s => (s.From.X + s.To.X) / 2f);
        var avgY = segs.Average(s => (s.From.Y + s.To.Y) / 2f);
        var avg = new Vector2(avgX, avgY);
        return avg;
    }
    public static IEnumerable<LineSegment> GetInscribed(this IEnumerable<LineSegment> segs, Vector2 center,
        float insetFactor)
    {
        return segs.Select(s => new LineSegment((s.From - center) * insetFactor, (s.To - center) * insetFactor));
    }
    public static List<LineSegment> FlipChainify(this List<LineSegment> lineSegments)
    {
        var hash = new HashSet<LineSegment>(lineSegments);
        
        var start = hash.First();
        hash.Remove(start);
        var tos = new List<LineSegment>();
        var froms = new List<LineSegment>();
        var to = start.To;
        var from = start.From;

        while (hash.Count > 0)
        {
            var nextTo = hash.FirstOrDefault(ls => ls.From == to || ls.To == to);
            if (nextTo == null) break;
            hash.Remove(nextTo);
            if (nextTo.To == to)
            {
                nextTo = nextTo.Reverse();
            }
            to = nextTo.To;
            tos.Add(nextTo);
        }

        while (hash.Count > 0)
        {
            var nextFrom = hash.FirstOrDefault(ls => ls.From == from || ls.To == from);
            if (nextFrom == null) break;
            hash.Remove(nextFrom);
            if (nextFrom.From == from)
            {
                nextFrom = nextFrom.Reverse();
            }
            from = nextFrom.From;
            froms.Add(nextFrom);
        }

        froms.Reverse();
        froms.Add(start);
        froms.AddRange(tos);
        
        if (hash.Count != 0)
        {
            var e = new GeometryException("chainification could not complete");
            e.AddSegLayer(lineSegments, "before");
            e.AddSegLayer(froms, "attempt");
            e.AddSegLayer(hash.ToList(), "leftover");
            
            throw e;
        }

        if (froms.IsChain() == false) throw new Exception();
        return froms; 
    }
    
    public static List<LineSegment> Chainify(this List<LineSegment> lineSegments)
    {
        var froms = new Dictionary<Vector2, LineSegment>();
        var tos = new Dictionary<Vector2, LineSegment>();
        Vector2 first = Vector2.Inf;
        for (var i = 0; i < lineSegments.Count; i++)
        {
            var seg = lineSegments[i];
            tos.Add(seg.To, seg);
        }
        for (var i = 0; i < lineSegments.Count; i++)
        {
            var seg = lineSegments[i];
            froms.Add(seg.From, seg);
            if (tos.ContainsKey(seg.From) == false) first = seg.From;
        }

        if (first == Vector2.Inf) first = lineSegments[0].From;

        var curr = froms[first];
        var res = new List<LineSegment>{curr};
        
        for (var i = 0; i < lineSegments.Count - 1; i++)
        {
            var next = froms[curr.To];
            res.Add(next);
            curr = next;
        }

        return res;
    }
    
    
    public static List<LineSegment> Chainify(this List<List<LineSegment>> chains)
    {
        var froms = new Dictionary<Vector2, List<LineSegment>>();
        var tos = new Dictionary<Vector2, List<LineSegment>>();
        Vector2 first = Vector2.Inf;
        for (var i = 0; i < chains.Count; i++)
        {
            var chain = chains[i];
            tos.Add(chain[chain.Count - 1].To, chain);
        }
        for (var i = 0; i < chains.Count; i++)
        {
            var chain = chains[i];
            froms.Add(chain[0].From, chain);
            if (tos.ContainsKey(chain[0].From) == false) first = chain[0].From;
        }

        if (first == Vector2.Inf) first = chains[0][0].From;

        var curr = froms[first];
        
        
        var res = new List<LineSegment>(curr);
        
        for (var i = 0; i < chains.Count - 1; i++)
        {
            var next = froms[curr[curr.Count - 1].To];
            res.AddRange(next);
            curr = next;
        }

        return res;
    }
    public static List<List<LineSegment>> GetChains(this List<LineSegment> segments)
    {
        if (segments.Count == 0) return new List<List<LineSegment>>();
        var dic = new Dictionary<Vector2, List<LineSegment>>();
        for (var i = 0; i < segments.Count; i++)
        {
            var seg = segments[i];
            dic.GetOrAdd(seg.From, v => new List<LineSegment>())
                .Add(seg);
            dic.GetOrAdd(seg.To, v => new List<LineSegment>())
                .Add(seg);
        }

        var joins = dic.Where(kvp => kvp.Value.Count > 2)
            .Select(kvp => kvp.Key)
            .ToHashSet();
        var res = new List<List<LineSegment>>();
        var handled = new HashSet<LineSegment>();
        var curr = segments[0];
        while (curr != null)
        {
            var list = new List<LineSegment> { curr };
            handled.Add(curr);
            go(curr, curr.To, list);
            go(curr, curr.From, list);
            res.Add(list);
            curr = segments.FirstOrDefault(ls => handled.Contains(ls) == false);
        }

        void go(LineSegment curr, Vector2 prev, List<LineSegment> list)
        {
            Vector2 point;
            if (curr.From == prev)
            {
                point = curr.To;
            }
            else if (curr.To == prev)
            {
                point = curr.From;
            }
            else throw new Exception();
            
            if (joins.Contains(point) 
                || dic[point].All(ls => handled.Contains(ls))
                || dic[point].Count != 2)
            {
                return;
            }

            var next = dic[point].First(ls => ls != curr);
            handled.Add(next);
            list.Add(next);
            go(next, point, list);
        }
        return res
            .Select(c => FlipChainify(c))
            .ToList();
    }

    public static void CompleteCircuit(this List<LineSegment> segs)
    {
        if(segs[segs.Count - 1].To != segs[0].From) segs.Add(new LineSegment(segs[segs.Count - 1].To, segs[0].From));
    }
    
    public static void SplitToMinLength(this MapPolygonEdge edge, float minLength, GenWriteKey key)
    {
        var newSegsAbs = new List<LineSegment>();
        var segs = edge.GetSegsAbs(key.Data);
        var offset = edge.HighPoly.Entity(key.Data).GetOffsetTo(edge.LowPoly.Entity(key.Data), key.Data);
        for (var i = 0; i < segs.Count; i++)
        {
            var seg = segs[i];
            var axis = (seg.To - seg.From);
            var l = seg.Length();
            if (l > minLength * 2f)
            {
                var numSplits = Mathf.FloorToInt(l / minLength) - 1;
                var prev = seg.From;
                for (int j = 1; j <= numSplits; j++)
                {
                    var interp = j / (numSplits + 1f);
                    var splitP = (seg.From + axis * interp).Intify();

                    newSegsAbs.Add(new LineSegment(prev, splitP));
                    prev = splitP;
                }

                newSegsAbs.Add(new LineSegment(prev, seg.To));
            }
            else
            {
                newSegsAbs.Add(seg);
            }
        }
        edge.ReplaceMiddlePoints(newSegsAbs, key);
    }
    
    public static IEnumerable<LineSegment> GetLineSegments(this List<Vector2> points, bool close = false)
    {
        return Enumerable.Range(0, points.Count() - (close ? 0 : 1))
            .Select(i =>
            {
                return new LineSegment(points[i], points.Modulo(i + 1));
            });
    }

    public static List<Vector2> GetPoints(this IEnumerable<LineSegment> pairs)
    {
        var first = pairs.First();
        var result = pairs
            .Select(pair => pair.From)
            .ToList();
        var last = pairs.Last();
        if(last.To != first.From) result.Add(last.To);
        return result;
    }
    public static float GetLength(this IEnumerable<LineSegment> pairs)
    {
        return pairs.Select(p => p.From.DistanceTo(p.To)).Sum();
    }
    
    public static Vector2 GetCornerPoint(this LineSegment l1, LineSegment l2, float thickness)
    {
        var angle = l1.AngleBetween(l2);
        var axis = l1.GetNormalizedAxis();
        var perp = -axis.Orthogonal() * thickness;
        if (angle > Mathf.Pi) angle = -angle;
        var d = thickness / Mathf.Tan(angle / 2f);
        var p = l1.To - axis * d + perp; 

        return p;
    }
    public static float AngleBetween(this LineSegment l1, LineSegment l2)
    {
        if (l1.To != l2.From) throw new Exception();
        return (l1.From - l1.To).GetClockwiseAngleTo(l2.To - l2.From);
    }

    public static Vector2 GetPointAlong(this List<LineSegment> segs, float ratio)
    {
        if (ratio < 0f || ratio > 1f) throw new Exception();
        var totalLength = segs.Sum(ls => ls.Length());
        var targetLength = ratio * totalLength;
        var soFar = 0f;
        for (var i = 0; i < segs.Count; i++)
        {
            var seg = segs[i];
            var toGo = targetLength - soFar;
            if (toGo > seg.Length())
            {
                soFar += seg.Length();
                continue;
            }

            return seg.From + seg.GetNormalizedAxis() * toGo;
        }

        return segs[segs.Count - 1].To;
    }
}