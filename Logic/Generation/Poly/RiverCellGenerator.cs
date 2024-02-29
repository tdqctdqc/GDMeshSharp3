
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
        var bankCellRemainders = new Vector2[bankCells.Length][];
        var halfRiverCellsEnum = bankCells
            .AsParallel()
            .SelectMany((c, i) => MakeEdgeRiverCells(c, i,
                bankCellRemainders,
                cellEdgeRiverWidths, key.Data))
            .ToArray();
        var halfRiverCells
             = new Dictionary<Vector2I, Vector2[]>(halfRiverCellsEnum);
        foreach (var (edgeKey, bound) in halfRiverCells)
        {
            var cell = cells[edgeKey.X];
            key.Data.ClientPlayerData.RiverCells.TryAdd(cell, new List<Vector2[]>());
            key.Data.ClientPlayerData.RiverCells[cell].Add(bound);
        }
            
        var wholeRiverCells = halfRiverCells
        .AsParallel()
        .Where(k => k.Key.X > k.Key.Y)
        .Select(v =>
        {
            var edgeKey = v.Key;
            var hiCell = (LandCell)PlanetDomainExt.GetPolyCell(v.Key.X, key.Data);
            var loCell = (LandCell)PlanetDomainExt.GetPolyCell(v.Key.Y, key.Data);
            var edge = hiCell.Polygon.Entity(key.Data).GetEdge(loCell.Polygon.Entity(key.Data), key.Data);

            var side1 = halfRiverCells[edgeKey];
            var side2 = halfRiverCells[new Vector2I(edgeKey.Y, edgeKey.X)]
                .Select(v => hiCell.RelTo.Offset(v + loCell.RelTo, key.Data))
                .ToArray();
            var unified = side1.TryUnifyPolygons(side2, out var bothSides);
            if (unified == false) throw new Exception();
            //have to slice them by cell edge correctly
            var riverCell = RiverCell.Construct(edge, hiCell.RelTo, bothSides, key);
            return new KeyValuePair<Vector2I, RiverCell>(edgeKey, riverCell);
        }).ToArray();
        foreach (var (edgeKey, edgeCell) in wholeRiverCells)
        {
            cells.Add(edgeCell.Id, edgeCell);
        }
        foreach (var (edgeKey, edgeCell) in wholeRiverCells)
        {
            edgeCell.MakeNeighbors(key);
        }
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
    public static IEnumerable<KeyValuePair<Vector2I, Vector2[]>> 
        MakeEdgeRiverCells(PolyCell c,
            int cellIndex, Vector2[][] cellRemainders,
            Dictionary<Vector2I, float> cellEdgeRiverWidths, 
            Data d)
    {
        if (c is LandCell l == false) throw new Exception();
        var poly = l.Polygon.Entity(d);
        
        
        for (var i = 0; i < l.Neighbors.Count; i++)
        {
            var n = PlanetDomainExt.GetPolyCell(l.Neighbors[i], d);
            var cellEdgeKey = new Vector2I(c.Id, n.Id);
            if (validEdge(c, n) == false) continue;
            if (n is LandCell lN == false) continue;
            var nPoly = lN.Polygon.Entity(d);
            var edge = l.Edges[i];
            var edgeFlow = poly.GetEdge(nPoly, d)
                .MoistureFlow;
            var edgeRiverWidth = River.GetWidthFromFlow(edgeFlow);
            
            var axis = edge.t - edge.f;
            var perp = Clockwise
                .GetPerpTowards(edge.f, edge.t, Vector2.Zero)
                .Normalized();
            
            var mutuals = c.Neighbors.Intersect(n.Neighbors)
                .Select(i => PlanetDomainExt.GetPolyCell(i, d)).ToArray();
            if (mutuals.Length > 2 || mutuals.Length == 0) throw new Exception();

            var trapezoidFrom = getMutual(edge.f) is PolyCell m1
                ? getTrapezoidPoint(m1)
                : getDanglingPoint();
            var trapezoidTo = getMutual(edge.t) is PolyCell m2
                ? getTrapezoidPoint(m2)
                : getDanglingPoint();
            
            // mb.AddTriRel(edge.f + c.RelTo, 
            //     edge.t + c.RelTo,
            //     trapezoidFrom + c.RelTo, color, relTo, d);

            var bounds = new[] { edge.f, edge.t, trapezoidTo, trapezoidFrom };
            
            
            foreach (var mutual in mutuals)
            {
                if (validEdge(c, mutual)) continue;
                
                var mEdge = c.GetEdgeRelWith(mutual);
                var shared = getShared(edge, mEdge);
                var trapezoidPoint = shared == edge.f 
                    ?  trapezoidFrom : trapezoidTo;
                var mExclusive = getExclusive(mEdge, shared);
                var mAxis = mExclusive - shared;
                var thickness = getRiverWidth(shared) / 2f;
                var mLength = Mathf.Min(
                    mEdge.Item1.DistanceTo(mEdge.Item2) / 3f, 
                    thickness);
                var mPerp = Clockwise.GetPerpTowards(mEdge.Item1,
                    mEdge.Item2, Vector2.Zero).Normalized();
                var mPoint = shared + mLength * mAxis.Normalized();
                
                var earBounds = new[] { mPoint, trapezoidPoint, shared };
                if (bounds.TryUnifyPolygons(earBounds, out var newBounds))
                {
                    bounds = newBounds;
                }
            }
            
            yield return new KeyValuePair<Vector2I, Vector2[]>(
                cellEdgeKey, bounds);

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
                var thickness = getRiverWidth(shared) / 2f;
                var mPerp = Clockwise.GetPerpTowards(mEdge.Item1,
                    mEdge.Item2, Vector2.Zero).Normalized();
                var mAxis = mEdge.Item2 - mEdge.Item1;
                var foundIntersect = Vector2Ext.LineSegIntersect(
                    edge.f + perp * thickness + axis * 10000f,
                    edge.f + perp * thickness - axis * 10000f,
                    mEdge.Item1 + mPerp * thickness + mAxis * 10000f,
                    mEdge.Item1 + mPerp * thickness - mAxis * 10000f,
                    false, out var intersect);
                if (foundIntersect == false) return shared + perp;
                return intersect;
            }

            Vector2 getDanglingPoint()
            {
                var shared = getShared(edge, c.GetEdgeRelWith(mutuals[0]));
                var dangling = shared == edge.t ? edge.f : edge.t;
                return dangling + perp;
            }

            float getRiverWidth(Vector2 p)
            {
                var mutual = getMutual(p);
                if (mutual == null) return edgeRiverWidth;
                var width = getEdgeRiverWidth(c, n);
                var iter = 1f;
                if (validEdge(c, mutual))
                {
                    width += getEdgeRiverWidth(c, mutual);
                    iter++;
                }

                if (validEdge(mutual, n))
                {
                    width += getEdgeRiverWidth(mutual, n);
                    iter++;
                }
                return width / iter;
            }
        }





        
        
        
        
        
        
        float getEdgeRiverWidth(PolyCell c1, PolyCell c2)
        {
            return cellEdgeRiverWidths[c1.GetIdEdgeKey(c2)];
        }
        
        bool validEdge(PolyCell p1, PolyCell p2)
        {
            if (p1 is LandCell l1 == false) return false;
            if (p2 is LandCell l2 == false) return false;
            if (l1.Polygon.RefId == l2.Polygon.RefId) return false;
            var edge = l1.Polygon.Entity(d)
                .GetEdge(l2.Polygon.Entity(d), d);
            if (edge.IsRiver() == false) return false;
            return true;
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