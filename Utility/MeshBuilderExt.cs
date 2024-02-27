
using System;
using System.Collections.Generic;
using System.Linq;
using Godot;

public static class MeshBuilderExt
{
    public static void DrawCellPath(this MeshBuilder mb,
        Vector2 relTo, List<PolyCell> path,
        Color color, float thickness, Data d)
    {
        for (var j = 0; j < path.Count - 1; j++)
        {
            var from = path[j].GetCenter();
            var to = path[j + 1].GetCenter();
            mb.AddArrow(relTo.Offset(from, d),
                relTo.Offset(to, d), thickness, color);
        }
    }
    public static void DrawFrontAssignment(this MeshBuilder mb,
        Vector2 relTo,
        HoldLineAssignment seg, 
        Data d)
    {
        var markerSize = 5f;
        var color = seg.Color;
        if (seg.Frontline.Faces.Count == 1)
        {
            var face = seg.Frontline.Faces[0];
            var cell = face.GetNative(d);
            mb.AddPoint(relTo.Offset(cell.GetCenter(), d),
                markerSize, color);
        }
        for (var i = 0; i < seg.Frontline.Faces.Count - 1; i++)
        {
            var face = seg.Frontline.Faces[i];
            var nextFace = seg.Frontline.Faces[i + 1];
            var from = face.GetNative(d);
            var to = nextFace.GetNative(d);
            
            mb.AddLine(relTo.Offset(from.GetCenter(),d),
                relTo.Offset(to.GetCenter(), d),
                color, markerSize);
        }

        foreach (var kvp in seg.GetLineAssignments(d))
        {
            var line = kvp.Value;
            var group = kvp.Key;
            for (var i = 0; i < line.Count; i++)
            {
                var face = seg.Frontline.Faces[i];
                var native = face.GetNative(d);
                var foreign = face.GetForeign(d);
                mb.AddArrow(relTo.Offset(native.GetCenter(),d),
                    relTo.Offset(foreign.GetCenter(), d),
                    markerSize / 5f, group.Color);
            }
            for (var i = 0; i < line.Count - 1; i++)
            {
                var a = line[i].GetNative(d);
                var b = line[i + 1].GetNative(d);
                if (a == b) continue;
                
                mb.AddLine(relTo.Offset(a.GetCenter(), d),
                    relTo.Offset(b.GetCenter(), d),
                    group.Color, markerSize / 2f);
            }
        }
    }

    public static void DrawPolyBorders(this MeshBuilder mb,
        Vector2 relTo, MapPolygon poly, float thickness, Data data)
    {
        var edgeBorders = poly
            .GetCells(data)
            .OfType<ISinglePolyCell>()
            .SelectMany(c => ((PolyCell)c).GetNeighbors(data)
                .OfType<ISinglePolyCell>()
                .Where(n => n.Polygon.RefId != poly.Id)
                .Select(n => (c, n)));
        foreach (var (c, n) in edgeBorders)
        {
            mb.DrawPolyCellEdge((PolyCell)c, (PolyCell)n, c => Colors.Black, 
                thickness, relTo, data);
        }
    }
    public static void DrawCellBorders(this MeshBuilder mb,
        Vector2 relTo, PolyCell cell, Data data, float thickness, 
        bool debug = false)
    {
        foreach (var n in cell.GetNeighbors(data))
        {
            mb.DrawPolyCellEdge(cell, n, c => Colors.Black, 
                thickness, relTo, data, debug);
        }
    }
    
    public static void DrawMovementRecord(this MeshBuilder mb,
        int id, int howFarBack, Vector2 relTo, Data d)
    {
        var records = d.Context.MovementRecords;
        if (records.ContainsKey(id))
        {
            var last = records[id]
                .TakeLast(howFarBack).ToList();
            if (last.Count() == 0) return;
            var tick = last[0].tick;
            var tickIter = 0;
            for (var i = 0; i < last.Count - 1; i++)
            {
                var from = last[i];
                var fromCell = PlanetDomainExt.GetPolyCell(from.cellId, d);
                var to = last[i + 1];
                var toCell = PlanetDomainExt.GetPolyCell(to.cellId, d);

                if (to.tick != tick)
                {
                    tick = to.tick;
                    tickIter++;
                }

                var color = ColorsExt.GetRainbowColor(tickIter);
                mb.AddArrow(relTo.Offset(fromCell.GetCenter(), d), 
                    relTo.Offset(toCell.GetCenter(), d), 2f, color);
            }
        }
    }

    public static void DrawPolygon(this MeshBuilder mb,
        Vector2[] boundaryPoints, Color color)
    {
        var tris = Geometry2D.TriangulatePolygon(boundaryPoints);
        for (var i = 0; i < tris.Length; i+=3)
        {
            var p1 = boundaryPoints[tris[i]];
            var p2 = boundaryPoints[tris[i+1]];
            var p3 = boundaryPoints[tris[i+2]];
            mb.AddTri(p1, p2, p3, color);
        }
    }
    public static void DrawPolygonRel(this MeshBuilder mb,
        Vector2[] boundaryPoints, Color color, Vector2 relTo, Data d)
    {
        var tris = Geometry2D.TriangulatePolygon(boundaryPoints);
        for (var i = 0; i < tris.Length; i+=3)
        {
            var p1 = relTo.Offset(boundaryPoints[tris[i]], d);
            var p2 = relTo.Offset(boundaryPoints[tris[i+1]], d);
            var p3 = relTo.Offset(boundaryPoints[tris[i+2]], d);
            mb.AddTri(p1, p2, p3, color);
        }
    }

    public static void DrawFrontFaces(this MeshBuilder mb,
        List<FrontFace> faces,
        Color color, 
        float thickness,
        Vector2 relTo, Data d)
    {
        for (var i = 0; i < faces.Count; i++)
        {
            var face = faces[i];
            var native = face.GetNative(d);
            var foreign = face.GetForeign(d);
            var nPos = relTo.Offset(native.GetCenter(), d);
            var fPos = relTo.Offset(foreign.GetCenter(), d);
            mb.AddArrow(nPos, fPos, thickness, color);
        }
    }
    public static void DrawRiverTestAll(this MeshBuilder mb,
        Data d, Vector2 relTo)
    {
        
        foreach (var cell in d.Planet.PolygonAux
                     .PolyCells.Cells.Values)
        {
            mb.DrawRiverTest(cell, d, relTo);
        }
    }
    public static void DrawRiverTestAround(this MeshBuilder mb,
        PolyCell c, Data d, Vector2 relTo)
    {
        if (c is LandCell l == false) return;
        var poly = l.Polygon.Entity(d);
        foreach (var cell in poly.GetCells(d))
        {
            mb.DrawRiverTest(cell, d, relTo);
        }
        foreach (var nPoly in poly.Neighbors.Items(d))
        {
            foreach (var cell in nPoly.GetCells(d))
            {
                mb.DrawRiverTest(cell, d, relTo);
            }
        }
    }
    public static void DrawRiverTest(this MeshBuilder mb,
        PolyCell c, Data d, Vector2 relTo)
    {
        if (c is LandCell l == false) return;
        var poly = l.Polygon.Entity(d);
        for (var i = 0; i < l.Neighbors.Count; i++)
        {
            var n = PlanetDomainExt.GetPolyCell(l.Neighbors[i], d);
            if (validEdge(c, n) == false) continue;
            var color = d.Models.Landforms.River.Color;
                
                ColorsExt.Rainbow.Modulo(c.Id + n.Id);
                ColorsExt.GetRandomColor();
                Colors.Blue.GetPeriodicShade(i);
            if (n is LandCell lN == false) continue;
            var nPoly = lN.Polygon.Entity(d);
            var edge = l.Edges[i];
            var edgeFlow = poly.GetEdge(nPoly, d)
                .MoistureFlow;
            var edgeRiverWidth = River.GetWidthFromFlow(edgeFlow);
            
            var axis = edge.t - edge.f;
            var perp = Clockwise.GetPerpTowards(edge.f, edge.t, Vector2.Zero)
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
            
            mb.AddTriRel(edge.f + c.RelTo, 
                edge.t + c.RelTo,
                trapezoidFrom + c.RelTo, color, relTo, d);
            mb.AddTriRel(trapezoidTo + c.RelTo, 
                edge.t + c.RelTo,
                trapezoidFrom + c.RelTo, color, relTo, d);
            
            foreach (var mutual in mutuals)
            {
                if (validEdge(c, mutual)) continue;
                
                var mEdge = c.GetEdgeRelWith(mutual);
                var shared = getShared(edge, mEdge);
                var trapezoidPoint = shared == edge.f 
                    ?  trapezoidFrom : trapezoidTo;
                var mExclusive = getExclusive(mEdge, shared);
                var mAxis = mExclusive - shared;
                var thickness = getRiverWidth(shared);
                var mLength = Mathf.Min(
                    mEdge.Item1.DistanceTo(mEdge.Item2) / 3f, 
                    thickness);
                var mPerp = Clockwise.GetPerpTowards(mEdge.Item1,
                    mEdge.Item2, Vector2.Zero).Normalized();
                var mPoint = shared + mLength * mAxis.Normalized();
                mb.AddTriRel(mPoint + c.RelTo, trapezoidPoint + c.RelTo, 
                    shared + c.RelTo,
                    color, relTo, d);
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
                var thickness = getRiverWidth(shared);
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
            var p1 = ((LandCell)c1).Polygon.Entity(d);
            var p2 = ((LandCell)c2).Polygon.Entity(d);
            
            var edgeFlow = p1.GetEdge(p2, d)
                .MoistureFlow;
            return River.GetWidthFromFlow(edgeFlow);
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