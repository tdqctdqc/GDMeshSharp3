
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Godot;

public class RiverCellGenerator
{
    public static void BuildRiverCells(
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
            .SelectMany(e => GetRiverCellEdgeWidths(e,nexusRiverWidths, key));
        var cellEdgeRiverWidths = new Dictionary<Vector2I, float>(
            cellEdgeRiverWidthsEnum);
        sw.Stop();
        GD.Print("river setup " + sw.Elapsed.TotalMilliseconds);
        sw.Reset();
        sw.Start();
        
        
        var riverCellsEnum = cellEdgeRiverWidths
            .Keys
            .AsParallel().Select(k =>
            {
                var c = (LandCell)cells[k.X];
                var n = (LandCell)cells[k.Y];
                
                var poly = c.Polygon.Entity(key.Data);
                var oPoly = n.Polygon.Entity(key.Data);
                var polyEdge = poly.GetEdge(oPoly, key.Data);
                
                var edge = c.GetEdgeRelWith(n);
                var edgeN = n.GetEdgeRelWith(c);
                if ((edgeN.Item1 + n.RelTo).ClampPosition(key.Data)
                     == (edge.Item1 + c.RelTo).ClampPosition(key.Data))
                {
                    
                }
                else if ((edgeN.Item2 + n.RelTo).ClampPosition(key.Data)
                         == (edge.Item1 + c.RelTo).ClampPosition(key.Data))
                {
                    edgeN = (edgeN.Item2, edgeN.Item1);
                }
                else throw new Exception();
                
                var bounds1 = GetCellEdgeRiverBounds(
                    c, 
                    edge.Item1, edge.Item2,
                    cellEdgeRiverWidths, n, key.Data);
                
                var bounds2 = GetCellEdgeRiverBounds(
                    n, 
                    edgeN.Item2, edgeN.Item1,
                    cellEdgeRiverWidths, c, key.Data)
                    .Select(v => c.RelTo.Offset(v + n.RelTo, key.Data));

                var united = bounds1.Concat(bounds2).ToArray();
                var rCell = RiverCell.Construct(polyEdge,
                    c.RelTo, united, key);
                return new KeyValuePair<Vector2I,RiverCell>
                    (new Vector2I(c.Id, n.Id), rCell);
            });

        var riverCells = new Dictionary<Vector2I, RiverCell>(riverCellsEnum);
        
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
        Dictionary<MapPolyNexus, float> nexusRiverWidths,
        GenWriteKey key)
    {
        var hiPoly = e.HighPoly.Entity(key.Data);
        var loPoly = e.LowPoly.Entity(key.Data);
        var hiNexus = e.HiNexus.Entity(key.Data);
        var loNexus = e.LoNexus.Entity(key.Data);
        var hiWidth = nexusRiverWidths[hiNexus];
        var loWidth = nexusRiverWidths[loNexus];

        var hiPres = key.GenData.GenAuxData
            .PreCellPolys[hiPoly];
        var hiCells = hiPres
            .Select(p => PlanetDomainExt.GetPolyCell(p.Id, key.Data))
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
    private static IEnumerable<Vector2> GetCellEdgeRiverBounds(Cell c, 
        Vector2 from, Vector2 to,
        Dictionary<Vector2I, float> riverCellEdgeWidths,
        Cell n, Data d)
    {
        var edge = c.GetEdgeRelWith(n);
        var edgeRiverWidth = riverCellEdgeWidths[c.GetIdEdgeKey(n)];
        var axis = to - from;
        var perp = Clockwise
            .GetPerpTowards(from, to, Vector2.Zero)
            .Normalized();
        var fromMutual = getMutual(from);
        var trapezoidFrom = fromMutual is Cell m1
            ? getTrapezoidPoint(m1)
            : getDanglingPoint();
        
        var toMutual = getMutual(to);
        var trapezoidTo = toMutual is Cell m2
            ? getTrapezoidPoint(m2)
            : getDanglingPoint();
        
        if (riverCellEdgeWidths.ContainsKey(c.GetIdEdgeKey(fromMutual))) fromMutual = null;
        if (riverCellEdgeWidths.ContainsKey(c.GetIdEdgeKey(toMutual))) toMutual = null;
        
        if (Vector2Ext.LineSegIntersect(from, trapezoidFrom,
                to, trapezoidTo, true, out var intersect))
        {
            var temp = trapezoidTo;
            trapezoidTo = trapezoidFrom;
            trapezoidFrom = temp;
        }

        if (fromMutual is not null && toMutual is not null)
        {
            yield return from; 
            yield return getMutualEarPoint(fromMutual);
            yield return trapezoidFrom;           
            yield return trapezoidTo;
            yield return getMutualEarPoint(toMutual);
        }
        else if (fromMutual is not null)
        {
            yield return from; 
            yield return getMutualEarPoint(fromMutual);
            yield return trapezoidFrom;           
            yield return trapezoidTo;
        }
        else if (toMutual is not null)
        {
            yield return from; 
            yield return trapezoidFrom;           
            yield return trapezoidTo;
            yield return getMutualEarPoint(toMutual);
        }
        else
        {
            yield return from; 
            yield return trapezoidFrom;           
            yield return trapezoidTo;
        }


        Vector2 getMutualEarPoint(Cell mutual)
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

        Cell getMutual(Vector2 edgePoint)
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
        
        Vector2 getTrapezoidPoint(Cell mutual)
        {
            var mEdge = c.GetEdgeRelWith(mutual);
            var shared = getShared(edge, mEdge);
            var thickness = getEdgeSideRiverWidth(shared) / 2f;
            var mPerp = Clockwise.GetPerpTowards(mEdge.Item1,
                mEdge.Item2, Vector2.Zero).Normalized();
            var mAxis = mEdge.Item2 - mEdge.Item1;
            var foundIntersect = Vector2Ext.LineSegIntersect(
                from + perp * thickness + axis * 10000f,
                from + perp * thickness - axis * 10000f,
                mEdge.Item1 + mPerp * thickness + mAxis * 10000f,
                mEdge.Item1 + mPerp * thickness - mAxis * 10000f,
                false, out var intersect);
            if (foundIntersect == false) return shared + perp;
            return intersect;
        }

        Vector2 getDanglingPoint()
        {
            if (from.Y == 0f)
            {
                return from + perp;
            }
            if (to.Y == 0f)
            {
                return to + perp;
            }
            if (from.Y > to.Y)
            {
                return from + perp;
            }
            return to + perp;
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
        float getEdgeRiverWidth(Cell c1, Cell c2)
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