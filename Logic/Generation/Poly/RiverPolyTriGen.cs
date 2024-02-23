using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Godot;

public class RiverPolyTriGen
{
    public TempRiverData DoRivers(GenWriteKey key)
    {
        var rd = new TempRiverData();
        // var lms = key.Data.Planet.PolygonAux.LandSea.Landmasses;
        // var riverNexi = key.Data.GetAll<MapPolyNexus>()
        //     .Where(n => n.IncidentEdges.Items(key.Data).Any(e => e.IsRiver())).ToList();
        // Parallel.ForEach(lms, lm => PreprocessRiversForLandmass(rd, riverNexi, lm.Polys, key));
        // key.Data.Notices.SetPolyShapes?.Invoke();
        return rd;
    }
    // private void PreprocessRiversForLandmass(TempRiverData rd, List<MapPolyNexus> riverNexi,
    //     HashSet<MapPolygon> lm, GenWriteKey key)
    // {
    //     var rIncidentEdges = riverNexi
    //         .Where(n => n.IncidentPolys.Items(key.Data).Any(p => lm.Contains(p)))
    //         .SelectMany(n => n.IncidentEdges.Items(key.Data))
    //         .Distinct()
    //         .ToList();
    //     MakePivots(rd, rIncidentEdges, key);
    // }
    // private void MakePivots(TempRiverData rd, List<MapPolygonEdge> rIncidentEdges, GenWriteKey key)
    // {
    //     foreach (var edge in rIncidentEdges)
    //     {
    //         var hiSegments = edge.HighSegsRel(key.Data).Segments;
    //         var hiPoly = edge.HighPoly.Entity(key.Data);
    //
    //         var fromTo = edge.OrderNexi(hiPoly, key.Data);
    //         MapPolyNexus fromNexus = fromTo.from;
    //         MapPolyNexus toNexus = fromTo.to;
    //         
    //         if (hiSegments.Count == 1)
    //         {
    //             MakePivotsSingleSeg(hiSegments, fromNexus, toNexus, edge, rd, key);
    //         }
    //         else
    //         {
    //             MakePivotsMultSegs(hiSegments, fromNexus, toNexus, edge, rd, key);   
    //         }
    //     }
    // }
    //
    // private void MakePivotsSingleSeg(List<LineSegment> hiSegments, MapPolyNexus fromNexus, MapPolyNexus toNexus,
    //     MapPolygonEdge edge, TempRiverData rd, GenWriteKey key)
    // {
    //     var seg = hiSegments[0];
    //     var segLength = seg.Length();
    //     var axis = seg.GetNormalizedAxis();
    //     var fromPivot = seg.From + axis * segLength * 1f / 3f;
    //     fromPivot = fromPivot.RoundTo2Digits();
    //     var toPivot = seg.From + axis * segLength * 2f / 3f;
    //     toPivot = toPivot.RoundTo2Digits();
    //             
    //     rd.HiPivots.TryAdd(new EdgeEndKey(fromNexus, edge), fromPivot);
    //     rd.HiPivots.TryAdd(new EdgeEndKey(toNexus, edge), toPivot);
    //     var offset = edge.HighPoly.Entity(key.Data).Center;
    //             
    //     var split = new List<LineSegment>
    //     {
    //         new LineSegment(seg.From, fromPivot).Translate(offset),
    //         new LineSegment(fromPivot, toPivot).Translate(offset),
    //         new LineSegment(toPivot, seg.To).Translate(offset)
    //     };
    //     edge.ReplaceMiddlePoints(split, key);
    // }
    //
    // private void MakePivotsMultSegs(List<LineSegment> hiSegments, MapPolyNexus fromNexus, MapPolyNexus toNexus,
    //     MapPolygonEdge edge, TempRiverData rd, GenWriteKey key)
    // {
    //     var newHiSegs = new List<LineSegment>();
    //     var hiPoly = edge.HighPoly.Entity(key.Data);
    //     var fromSeg = hiSegments[0];
    //     var fromPivotWidth = fromNexus.IncidentEdges.Items(key.Data).Average(e => River.GetWidthFromFlow(e.MoistureFlow)) / 2f;
    //     var fromSegWidth = fromSeg.Length();
    //     if (fromPivotWidth + 10f >= fromSegWidth)
    //     {
    //         rd.HiPivots.TryAdd(new EdgeEndKey(fromNexus, edge), fromSeg.To);
    //         newHiSegs.Add(fromSeg.Copy());
    //     }
    //     else
    //     {
    //         var pivot = fromSeg.From + fromSeg.GetNormalizedAxis() * fromPivotWidth;
    //         pivot = pivot.RoundTo2Digits();
    //         rd.HiPivots.TryAdd(new EdgeEndKey(fromNexus, edge), pivot);
    //         var s1 = new LineSegment(fromSeg.From, pivot);
    //         if (s1.Length() != 0f) newHiSegs.Add(s1);
    //         var s2 = new LineSegment(pivot, fromSeg.To);
    //         if (s2.Length() != 0f) newHiSegs.Add(s2);
    //     }
    //     for (var i = 1; i < hiSegments.Count - 1; i++)
    //     {
    //         newHiSegs.Add(hiSegments[i]);
    //     }
    //     
    //     var toSeg = hiSegments[hiSegments.Count - 1];
    //     var toPivotWidth = toNexus.IncidentEdges.Items(key.Data)
    //         .Average(e => River.GetWidthFromFlow(e.MoistureFlow)) / 2f;
    //     var toSegWidth = toSeg.Length();
    //     if (toPivotWidth + 10f >= toSegWidth)
    //     {
    //         rd.HiPivots.TryAdd(new EdgeEndKey(toNexus, edge), toSeg.From);
    //         newHiSegs.Add(toSeg.Copy());
    //     }
    //     else
    //     {
    //         var pivot = toSeg.To - toSeg.GetNormalizedAxis() * toPivotWidth;
    //         pivot = pivot.RoundTo2Digits();
    //         rd.HiPivots.TryAdd(new EdgeEndKey(toNexus, edge), pivot);
    //         var s1 = new LineSegment(toSeg.From, pivot);
    //         if (s1.Length() != 0f) newHiSegs.Add(s1);
    //         var s2 = new LineSegment(pivot, toSeg.To);
    //         if (s2.Length() != 0f) newHiSegs.Add(s2);
    //     }
    //     
    //     for (var i = 0; i < newHiSegs.Count; i++)
    //     {
    //         var seg = newHiSegs[i];
    //         var newFrom = seg.From + hiPoly.Center;
    //         var newTo = seg.To + hiPoly.Center;
    //         seg.From = newFrom;
    //         seg.To = newTo;
    //     }
    //     
    //     edge.ReplaceMiddlePoints(newHiSegs, key);
    // }
}