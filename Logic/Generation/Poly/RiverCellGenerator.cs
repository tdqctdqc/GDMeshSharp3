
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
        var cells = key.Data.Planet.PolygonAux
            .PolyCells.Cells;
        
        var nexusRiverWidths = nexi
            .AsParallel()
            .Where(n => n.IsRiverNexus(key.Data))
            .ToDictionary(
                n => n,
                n => n.IncidentEdges.Items(key.Data)
                    .Where(e => e.IsRiver())
                    .Average(e => River.GetWidthFromFlow(e.MoistureFlow)));
        
        var riverPolyEdges = key.Data.GetAll<MapPolygonEdge>()
            .Where(e => e.IsRiver());
        var cellEdgeRiverWidthsEnum 
            = riverPolyEdges
            .AsParallel()
            .SelectMany(e => GetRiverCellEdgeWidths(e, 
                cellsByPoly, nexusRiverWidths, key));
        var cellEdgeRiverWidths = new Dictionary<Vector2I, float>(
            cellEdgeRiverWidthsEnum);
        sw.Stop();
        GD.Print("river setup " + sw.Elapsed.TotalMilliseconds);
        sw.Reset();
        sw.Start();
        
        var hiCellBounds = cellEdgeRiverWidths
            .Keys
            .AsParallel()
            .Select(k =>
            {
                var c = cells[k.X];
                var n = cells[k.Y];
                var bounds = GetCellEdgeRiverBounds(c, cellEdgeRiverWidths, n, key.Data);
                return new KeyValuePair<Vector2I,Vector2[]>(new Vector2I(c.Id, n.Id), bounds);
            });
        var loCellBounds = cellEdgeRiverWidths
            .Keys
            .AsParallel()
            .Select(k =>
            {
                var c = cells[k.Y]; 
                var n = cells[k.X]; 
                var bounds = GetCellEdgeRiverBounds(c, cellEdgeRiverWidths, n, key.Data);
                return new KeyValuePair<Vector2I,Vector2[]>(new Vector2I(c.Id, n.Id), bounds);
            });
        var els = hiCellBounds.Concat(loCellBounds);
        var halfCellDic = new Dictionary<Vector2I, Vector2[]>
            (els);
        
        sw.Stop();
        GD.Print("make half bounds " + sw.Elapsed.TotalMilliseconds);
        sw.Reset();
        sw.Start();
        
        var riverCells = halfCellDic
            .AsParallel()
            .Where(kvp =>
            {
                var edgeKey = kvp.Key;
                return edgeKey.X > edgeKey.Y;
            })
            .ToDictionary(kvp => kvp.Key,
                kvp =>
            {
                var edgeKey = kvp.Key;
                var cell = (LandCell)cells[edgeKey.X];
                var oppositeCell = (LandCell)cells[edgeKey.Y];
                var poly = cell.Polygon.Entity(key.Data);
                var oPoly = oppositeCell.Polygon.Entity(key.Data);
                var polyEdge = poly.GetEdge(oPoly, key.Data);
                var oppositeBounds = halfCellDic[new Vector2I(edgeKey.Y, edgeKey.X)];
                oppositeBounds = oppositeBounds
                    .Select(v => cell.RelTo.Offset(v + oppositeCell.RelTo, key.Data)).ToArray();

                var united = kvp.Value.UnifyPolygons(oppositeBounds);
                return RiverCell.Construct(polyEdge, cell.RelTo,
                    united, key);
            });
        
        
        foreach (var (edgeKey, riverCell) in riverCells)
        {
            cells.Add(riverCell.Id, riverCell);
        }
        foreach (var (edgeKey, riverCell) in riverCells)
        {
            riverCell.MakeNeighbors(edgeKey, riverCells, key);
        }
        
        sw.Stop();
        GD.Print("combine halves and make cells time " + sw.Elapsed.TotalMilliseconds);
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

        // var hiPres = key.GenData.GenAuxData
        //     .PreCellPolys[hiPoly].Select();
        
        
        
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
    private static Vector2[] GetCellEdgeRiverBounds(PolyCell c, 
        Dictionary<Vector2I, float> riverCellEdgeWidths,
        PolyCell n, Data d)
    {
        var edge = c.GetEdgeRelWith(n);
        var edgeRiverWidth = riverCellEdgeWidths[c.GetIdEdgeKey(n)];
        var axis = edge.Item2 - edge.Item1;
        var perp = Clockwise
            .GetPerpTowards(edge.Item1, edge.Item2, Vector2.Zero)
            .Normalized();
        var fromMutual = getMutual(edge.Item1);
        var trapezoidFrom = fromMutual is PolyCell m1
            ? getTrapezoidPoint(m1)
            : getDanglingPoint();
        
        var toMutual = getMutual(edge.Item2);
        var trapezoidTo = toMutual is PolyCell m2
            ? getTrapezoidPoint(m2)
            : getDanglingPoint();
        
        if (riverCellEdgeWidths.ContainsKey(c.GetIdEdgeKey(fromMutual))) fromMutual = null;
        if (riverCellEdgeWidths.ContainsKey(c.GetIdEdgeKey(toMutual))) toMutual = null;
        
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
            if (riverCellEdgeWidths.ContainsKey(c.GetIdEdgeKey(mutual))) return shared;
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
            for (var i = 0; i < c.Neighbors.Count; i++)
            {
                var mId = c.Neighbors[i];
                if (n.Neighbors.Contains(mId) == false) continue;
                var mEdge = c.Edges[i];
                if (mEdge.Item1 == edgePoint || mEdge.Item2 == edgePoint)
                {
                    return PlanetDomainExt.GetPolyCell(mId, d);
                }
            }

            return null;
        }
        
        Vector2 getTrapezoidPoint(PolyCell mutual)
        {
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
            if (edge.Item1.Y == 0f)
            {
                return edge.Item1 + perp;
            }

            if (edge.Item2.Y == 0f)
            {
                return edge.Item2 + perp;
            }

            if (edge.Item1.Y > edge.Item2.Y)
            {
                return edge.Item1 + perp;
            }
            return edge.Item2 + perp;
        }

        float getEdgeSideRiverWidth(Vector2 p)
        {
            var mutual = getMutual(p);
            if (mutual == null) return edgeRiverWidth;
            var width = getEdgeRiverWidth(c, n);
            var iter = 1f;
            if (riverCellEdgeWidths.ContainsKey(c.GetIdEdgeKey(mutual)))
            {
                width += getEdgeRiverWidth(c, mutual);
                iter++;
            }

            if (riverCellEdgeWidths.ContainsKey(mutual.GetIdEdgeKey(n)))
            {
                width += getEdgeRiverWidth(mutual, n);
                iter++;
            }
            return width / iter;
        }
        float getEdgeRiverWidth(PolyCell c1, PolyCell c2)
        {
            return riverCellEdgeWidths[c1.GetIdEdgeKey(c2)];
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
}