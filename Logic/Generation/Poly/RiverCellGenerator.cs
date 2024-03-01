
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Godot;

public class RiverCellGenerator
{
    public static void BuildRiverCells(
        Dictionary<MapPolygon, PolyCell[]> cellsByPoly, 
        GenWriteKey key)
    {
        var sw = new Stopwatch();
        sw.Start();
        var nexi = key.Data.GetAll<MapPolyNexus>();
        var edges = key.Data.GetAll<MapPolygonEdge>();
        var cells = key.Data.Planet.PolygonAux.PolyCells.Cells;
        var nexusRiverWidths = nexi
            .Where(n => n.IsRiverNexus(key.Data))
            .ToDictionary(
                n => n,
                n => n.IncidentEdges.Items(key.Data)
                    .Where(e => e.IsRiver())
                    .Average(e => River.GetWidthFromFlow(e.MoistureFlow)));
        
        var riverPolyEdges = key.Data.GetAll<MapPolygonEdge>()
            .Where(e => e.IsRiver()).ToArray();
        var cellEdgeRiverWidthsEnum = riverPolyEdges
            .AsParallel()
            .SelectMany(e => GetRiverCellEdgeWidths(e, cellsByPoly, nexusRiverWidths, key));
        var cellEdgeRiverWidths = new Dictionary<Vector2I, float>(
            cellEdgeRiverWidthsEnum);
        var bankCells = cells.Values
            .AsParallel()
            .OfType<LandCell>()
            .Where(c => c.Neighbors.Any(n => cellEdgeRiverWidths.ContainsKey(c.GetIdEdgeKey(n))))
            .ToArray();
        
        sw.Stop();
        GD.Print("river setup " + sw.Elapsed.TotalMilliseconds);
        sw.Reset();
        sw.Start();
        
        
                
        //grab remaining boundary of bank cells here too 
        var halfRiverCellBounds = bankCells
            .AsParallel()
            .SelectMany(c => GetCellRiverBounds(c, key));
        var halfCellDic = new Dictionary<Vector2I, Vector2[]>(halfRiverCellBounds);
        var riverCells = halfCellDic
            .AsParallel()
            .Where(kvp =>
            {
                var edgeKey = kvp.Key;
                return edgeKey.X > edgeKey.Y;
            })
            .Select(kvp =>
            {
                var edgeKey = kvp.Key;
                var cell = (LandCell)PlanetDomainExt.GetPolyCell(edgeKey.X, key.Data);
                var oppositeCell = (LandCell)PlanetDomainExt.GetPolyCell(edgeKey.Y, key.Data);
                var poly = cell.Polygon.Entity(key.Data);
                var oPoly = oppositeCell.Polygon.Entity(key.Data);
                var polyEdge = poly.GetEdge(oPoly, key.Data);
                var oppositeBounds = halfCellDic[new Vector2I(edgeKey.Y, edgeKey.X)];
                oppositeBounds = oppositeBounds
                    .Select(v => cell.RelTo.Offset(v + oppositeCell.RelTo, key.Data)).ToArray();

                var united = kvp.Value.UnifyPolygons(oppositeBounds);
                return RiverCell.Construct(polyEdge, cell.RelTo,
                    united, key);
            }).ToArray();
        
        
        
        
        foreach (var riverCell in riverCells)
        {
            cells.Add(riverCell.Id, riverCell);
            
            key.Data.ClientPlayerData.RiverCells.Add(riverCell, new List<Vector2[]> { riverCell.RelBoundary });

        }
        // foreach (var (edgeKey, edgeCell) in wholeRiverCells)
        // {
        //     edgeCell.MakeNeighbors(key);
        // }
        sw.Stop();
        GD.Print("river make cells time " + sw.Elapsed.TotalMilliseconds);
    }
    
    private static IEnumerable<KeyValuePair<Vector2I, float>> 
        GetRiverCellEdgeWidths(
        MapPolygonEdge e, 
        Dictionary<MapPolygon, PolyCell[]> cellsByPoly, 
        Dictionary<MapPolyNexus, float> nexusRiverWidths,
        GenWriteKey key)
    {
        var hiPoly = e.HighPoly.Entity(key.Data);
        var loPoly = e.LowPoly.Entity(key.Data);
        var hiNexus = e.HiNexus.Entity(key.Data);
        var loNexus = e.LoNexus.Entity(key.Data);
        var hiWidth = nexusRiverWidths[hiNexus];
        var loWidth = nexusRiverWidths[loNexus];
        var hiCells = cellsByPoly[hiPoly]
            .Where(c => c.GetNeighbors(key.Data)
                .OfType<LandCell>()
                .Any(n => n.Polygon.RefId == loPoly.Id));
        foreach (var c in hiCells)
        {
            foreach (var n in c.GetNeighbors(key.Data))
            {
                if (n is LandCell l && l.Polygon.RefId == loPoly.Id)
                {
                    var edge = c.GetEdgeRelWith(n);
                    var mid = c.RelTo + (edge.Item1 + edge.Item2) / 2f;
                    var distToHi = hiNexus.Point.Offset(mid, key.Data).Length();
                    var distToLo = hiNexus.Point.Offset(mid, key.Data).Length();
                    var totalDist = distToHi + distToLo;
                    if (totalDist == 0f) throw new Exception();
                    var hiRatio = (totalDist - distToHi) / totalDist;
                    var loRatio = (totalDist - distToLo) / totalDist;
                    var width = (hiWidth * hiRatio + loWidth * loRatio);
                    yield return new KeyValuePair<Vector2I, float>(c.GetIdEdgeKey(n), width);
                }
            }
        }
    }


    private static IEnumerable<KeyValuePair<Vector2I, Vector2[]>>
        GetCellRiverBounds(PolyCell c, GenWriteKey key)
    {
        var cellBounds = c.RelBoundary;
        foreach (var n in c.GetNeighbors(key.Data))
        {
            if (IsRiverEdge(c, n, key.Data))
            {
                var edgeKey = new Vector2I(c.Id, n.Id);
                var riverEdgeBounds = GetCellEdgeRiverBounds(c, n, key.Data);
                yield return new KeyValuePair<Vector2I, Vector2[]>(edgeKey,
                    riverEdgeBounds);
            }
        }
        // c.SetBoundaryPoints(cellBounds, key);
    }
    private static Vector2[] GetCellEdgeRiverBounds(PolyCell c, 
        PolyCell n, Data d)
    {
        if (c is LandCell l == false) throw new Exception();
        var poly = l.Polygon.Entity(d);
        if (n is LandCell lN == false) throw new Exception();
        var nPoly = lN.Polygon.Entity(d);
        var edge = c.GetEdgeRelWith(n);
        var edgeFlow = poly.GetEdge(nPoly, d)
            .MoistureFlow;
        var edgeRiverWidth = River.GetWidthFromFlow(edgeFlow);
        
        var axis = edge.Item2 - edge.Item1;
        var perp = Clockwise
            .GetPerpTowards(edge.Item1, edge.Item2, Vector2.Zero)
            .Normalized();
        
        var mutuals = c.Neighbors.Intersect(n.Neighbors)
            .Select(i => PlanetDomainExt.GetPolyCell(i, d)).ToArray();
        if (mutuals.Length > 2 || mutuals.Length == 0) throw new Exception();

        var fromMutual = getMutual(edge.Item1);
        var trapezoidFrom = fromMutual is PolyCell m1
            ? getTrapezoidPoint(m1)
            : getDanglingPoint();
        
        var toMutual = getMutual(edge.Item2);
        var trapezoidTo = toMutual is PolyCell m2
            ? getTrapezoidPoint(m2)
            : getDanglingPoint();
        
        if (IsRiverEdge(c, fromMutual, d)) fromMutual = null;
        if (IsRiverEdge(c, toMutual, d)) toMutual = null;
        
        if (Vector2Ext.LineSegIntersect(edge.Item1, trapezoidFrom,
                edge.Item2, trapezoidTo, true, out var intersect))
        {
            var temp = trapezoidTo;
            trapezoidTo = trapezoidFrom;
            trapezoidFrom = temp;
        }

        if (fromMutual is not null && toMutual is not null)
        {
            return new Vector2[]
            {
                edge.Item1, edge.Item2, getMutualEarPoint(toMutual),
                trapezoidTo, trapezoidFrom, getMutualEarPoint(fromMutual)
            };
        }

        if (fromMutual is not null)
        {
            return new Vector2[]
            {
                edge.Item1, edge.Item2,
                trapezoidTo, trapezoidFrom, getMutualEarPoint(fromMutual)
            };
        }
        
        if (toMutual is not null)
        {
            return new Vector2[]
            {
                edge.Item1, edge.Item2, getMutualEarPoint(toMutual),
                trapezoidTo, trapezoidFrom
            };
        }
        
        return new Vector2[]
        {
            edge.Item1, edge.Item2,
            trapezoidTo, trapezoidFrom
        };
        

        Vector2 getMutualEarPoint(PolyCell mutual)
        {
            var mEdge = c.GetEdgeRelWith(mutual);
            var shared = getShared(edge, mEdge);
            if (IsRiverEdge(c, mutual, d)) return shared;
            var mExclusive = getExclusive(mEdge, shared);
            var mAxis = mExclusive - shared;
            var thickness = getEdgeSideRiverWidth(shared) / 2f;
            var mLength = Mathf.Min(
                mEdge.Item1.DistanceTo(mEdge.Item2) / 3f, 
                thickness);
            var mPerp = Clockwise.GetPerpTowards(mEdge.Item1,
                mEdge.Item2, Vector2.Zero).Normalized();
            var mPoint = shared + mLength * mAxis.Normalized();
            return mPoint;
        }

        PolyCell getMutual(Vector2 edgePoint)
        {
            for (var j = 0; j < mutuals.Length; j++)
            {
                var mEdge = c.GetEdgeRelWith(mutuals[j]);
                if (mEdge.Item1 == edgePoint || mEdge.Item2 == edgePoint)
                {
                    return mutuals[j];
                }
            }

            return null;
        }
        
        Vector2 getTrapezoidPoint(PolyCell mutual)
        {
            //thickness will be just c-ns edge thickness if m-n is not river
            //else will be average of c-n, m-n
            var mEdge = c.GetEdgeRelWith(mutual);
            var shared = getShared(edge, mEdge);
            var thickness = getEdgeSideRiverWidth(shared) / 2f;
            var mPerp = Clockwise.GetPerpTowards(mEdge.Item1,
                mEdge.Item2, Vector2.Zero).Normalized();
            var mAxis = mEdge.Item2 - mEdge.Item1;
            var foundIntersect = Vector2Ext.LineSegIntersect(
                edge.Item1 + perp * thickness + axis * 10000f,
                edge.Item1 + perp * thickness - axis * 10000f,
                mEdge.Item1 + mPerp * thickness + mAxis * 10000f,
                mEdge.Item1 + mPerp * thickness - mAxis * 10000f,
                false, out var intersect);
            if (foundIntersect == false) return shared + perp;
            return intersect;
        }

        Vector2 getDanglingPoint()
        {
            var shared = getShared(edge, c.GetEdgeRelWith(mutuals[0]));
            var dangling = shared == edge.Item2 ? edge.Item1 : edge.Item2;
            return dangling + perp;
        }

        float getEdgeSideRiverWidth(Vector2 p)
        {
            var mutual = getMutual(p);
            if (mutual == null) return edgeRiverWidth;
            var width = getEdgeRiverWidth(c, n);
            var iter = 1f;
            if (IsRiverEdge(c, mutual, d))
            {
                width += getEdgeRiverWidth(c, mutual);
                iter++;
            }

            if (IsRiverEdge(mutual, n, d))
            {
                width += getEdgeRiverWidth(mutual, n);
                iter++;
            }
            return width / iter;
        }
        float getEdgeRiverWidth(PolyCell c1, PolyCell c2)
        {
            var p1 = ((LandCell)c1).Polygon.Entity(d);
            var p2 = ((LandCell)c2).Polygon.Entity(d);
            
            var edgeFlow = p1.GetEdge(p2, d)
                .MoistureFlow;
            return River.GetWidthFromFlow(edgeFlow);
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
    
    private static bool IsRiverEdge(PolyCell p1, PolyCell p2,
        Data d)
    {
        if (p1 is LandCell l1 == false) return false;
        if (p2 is LandCell l2 == false) return false;
        if (l1.Polygon.RefId == l2.Polygon.RefId) return false;
        var edge = l1.Polygon.Entity(d)
            .GetEdge(l2.Polygon.Entity(d), d);
        if (edge.IsRiver() == false) return false;
        return true;
    }
}