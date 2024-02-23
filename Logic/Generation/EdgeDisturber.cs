// using Godot;
// using System;
// using System.Collections.Generic;
// using System.Linq;
//
// public static class EdgeDisturber
// {
//     
//     public static void SplitEdges(IReadOnlyCollection<MapPolygon> polys, GenWriteKey key, float minLength)
//     {
//         int iter = 0;
//         foreach (var edge in key.Data.GetAll<MapPolygonEdge>())
//         {
//             // if (poly.IsWater() && n.IsWater()) continue;
//
//             if (edge.HighSegsRel(key.Data).Segments.Any(s => s.Length() > minLength * 2f))
//             {
//                 iter++;
//                 edge.SplitToMinLength(minLength, key);
//             }
//         }
//         key.Data.Notices.SetPolyShapes.Invoke();
//     }
//     
//     public static void DisturbEdges(IReadOnlyCollection<MapPolygon> polys, GenWriteKey key)
//     {
//         var disturbedEdges = new HashSet<Vector2>();
//         for (var i = 0; i < polys.Count; i++)
//         {
//             var poly = polys.ElementAt(i);
//             for (var j = 0; j < poly.Neighbors.Count(); j++)
//             {
//                 var nPoly = poly.Neighbors.Items(key.Data).ElementAt(j);
//                 if (poly.Id > nPoly.Id)
//                 {
//                     DisturbEdge(poly.GetEdge(nPoly, key.Data), key);
//                 }
//             }
//         }
//         key.Data.Notices.SetPolyShapes.Invoke();
//     }
//
//     private static void DisturbEdge(MapPolygonEdge edge, GenWriteKey key)
//     {
//         var maxRatio = .3f;
//         var minRatio = .2f;
//         var ratio = Game.I.Random.RandfRange(minRatio, maxRatio);
//         var points = edge.HighSegsRel(key.Data).Segments.GetPoints();
//         var newPoints = new List<Vector2>();
//         newPoints.Add(points[0]);
//         var hi = edge.HighPoly.Entity(key.Data);
//         var lo = edge.LowPoly.Entity(key.Data);
//         var offset = hi.GetOffsetTo(lo, key.Data);
//
//         for (var i = 1; i < points.Count - 1; i++)
//         {
//             var point = points[i];
//             bool disturbToHi = Game.I.Random.Randf() > .5f;
//             if (disturbToHi)
//             {
//                 var newP = point.Normalized() * (point.Length() * (1f - ratio));
//                 newP = newP.RoundTo2Digits();
//                 newPoints.Add(newP);
//             }
//             else
//             {
//                 var axis = (offset - point).Normalized() * ratio;
//                 var newP = point + axis;
//                 newP = newP.RoundTo2Digits();
//                 newPoints.Add(newP);
//             }
//         }
//         newPoints.Add(points[points.Count - 1]);
//
//         var newSegs = newPoints.GetLineSegments()
//             .Select(p => p.Translate(hi.Center))
//             .Where(ls => ls.From != ls.To)
//             .ToList();
//         edge.ReplaceMiddlePoints(newSegs, key);
//     }
// }
