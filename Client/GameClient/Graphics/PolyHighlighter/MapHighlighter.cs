using System;
using System.Collections.Generic;
using System.Linq;
using Godot;

public partial class MapHighlighter : Node2D
{
    private List<MeshInstance2D> _mis;

    public MapHighlighter(Data data)
    {
        Game.I.Client.UiRequests.MouseOver
            .SubscribeForNode(pos => DrawPoly(data, pos), this);
        _mis = new List<MeshInstance2D>();
    }
    private MapHighlighter()
    {
    }
    public enum Modes
    {
        Simple,
        Complex
    }


    public void DrawFrontSegment(FrontSegmentAssignment seg, Data d)
    {
        // var relTo = seg.GetTacWaypoints(d).First().Pos;
        // Position = Game.I.Client.Cam().GetMapPosInGlobalSpace(relTo);
        Position = Vector2.Zero;
        var mb = new MeshBuilder();
        var groups = seg.Groups(d);
        foreach (var group in groups)
        {
            var pos = group.GetPosition(d);
            mb.AddSquare(Game.I.Client.Cam().GetMapPosInGlobalSpace(pos), 10f, Colors.Red);
        }
        TakeFromMeshBuilder(mb);
    }
    public void DrawPoly(Data data, PolyTriPosition pos)
    {
        Visible = true;
        Clear();
        var poly = pos.Poly(data);
        var pt = pos.Tri(data);
        Position = Game.I.Client.Cam().GetMapPosInGlobalSpace(poly.Center);

        var mb = new MeshBuilder();
        
        var mode = Game.I.Client.Settings.PolyHighlightMode.Value;
        if (mode == Modes.Simple)
        {
            DrawPolySimple(data, poly, pt, mb);
        }
        else if (mode == Modes.Complex)
        {
            DrawPolyComplex(data, pos, poly, pt, mb);
        }
        else throw new Exception();
        
        TakeFromMeshBuilder(mb);
    }

    private void DrawPolySimple(Data data, MapPolygon poly, PolyTri pt, MeshBuilder mb)
    {
        DrawBordersSimple(poly, mb, data);
        // DrawAssocWaypoints(poly, mb, data);
        // DrawnNeighborBordersSimple(poly, mb, data);
    }

    private static void DrawPolyComplex(Data data, PolyTriPosition pos, MapPolygon poly, PolyTri pt, MeshBuilder mb)
    {
        DrawBoundarySegments(poly, mb, data);
        DrawPolyTriBorders(poly, mb, data);
    }

    private static void DrawIncidentEdges(MapPolygon poly, MeshBuilder mb, Data data)
    {
        var incident = new HashSet<MapPolygonEdge>();
        var edges = poly.Neighbors.Items(data).Select(n => poly.GetEdge(n, data));
        foreach (var e in edges)
        {
            var n1 = e.HiNexus.Entity(data).IncidentEdges.Items(data);
            var n2 = e.LoNexus.Entity(data).IncidentEdges.Items(data);
            incident.AddRange(n1);
            incident.AddRange(n2);
        }
        foreach (var e in incident)
        {
            var start = e.HiNexus.Entity(data).Point;
            var end = e.LoNexus.Entity(data).Point;
            mb.AddLine(poly.GetOffsetTo(start, data), 
                poly.GetOffsetTo(end, data), Colors.Red, 10f);
        }
    }
    private static void DrawBordersSimple(MapPolygon poly, MeshBuilder mb, Data data)
    {
        var edgeBorders = poly.GetOrderedBoundarySegs(data);
        mb.AddLines(edgeBorders, 2f, Colors.Black);
    }
    private static void DrawnLinesToNeighbors(MapPolygon poly, MeshBuilder mb, Data data)
    {
        foreach (var n in poly.Neighbors.Items(data))
        {
            var offset = poly.GetOffsetTo(n, data);
            mb.AddLine(Vector2.Zero, offset, Colors.White, 10f);
        }
    }
    private static void DrawnNeighborBordersSimple(MapPolygon poly, MeshBuilder mb, Data data)
    {
        foreach (var n in poly.Neighbors.Items(data))
        {
            var offset = poly.GetOffsetTo(n, data);
            var nEdgeBorders = n.GetOrderedBoundarySegs(data)
                .Select(s => s.Translate(offset)).ToList();
            mb.AddLines(nEdgeBorders, 10f, Colors.Black);
        }
    }
    private static void DrawNeighborBorders(MapPolygon poly, MeshBuilder mb, Data data)
    {
        var edgeBorders = poly.GetOrderedBoundarySegs(data);
        mb.AddArrowsRainbow(edgeBorders, 5f);
        mb.AddNumMarkers(edgeBorders.Select(ls => ls.Mid()).ToList(), 20f, 
            Colors.Transparent, Colors.White, Vector2.Zero);
    }
    private static void DrawBoundarySegments(MapPolygon poly, MeshBuilder mb, Data data)
    {
        var lines = poly.GetOrderedBoundarySegs(data);
        mb.AddArrowsRainbow(lines.ToList(), 5f);
        mb.AddNumMarkers(lines.Select(ls => ls.Mid()).ToList(), 20f, 
            Colors.Transparent, Colors.White, Vector2.Zero);
    }
    private static void DrawPolyTriBorders(MapPolygon poly, MeshBuilder mb, Data data)
    {
        var col = Colors.Black;
        foreach (var t in poly.Tris.Tris)
        {
            var inscribed = t.GetInscribed(.9f);
            mb.AddArrow(inscribed.A, inscribed.B, 1f, col);
            mb.AddArrow(inscribed.B, inscribed.C, 1f, col);
            mb.AddArrow(inscribed.C, inscribed.A, 1f, col);
        }
    }
    

    private void TakeFromMeshBuilder(MeshBuilder mb)
    {
        if (mb.Tris.Count == 0) return;
        var mi = mb.GetMeshInstance();
        mb.Clear();
        AddChild(mi);
        _mis.Add(mi);
    }
    public void Clear()
    {
        _mis.ForEach(mi =>
        {
            RemoveChild(mi);
            mi?.QueueFree();
            mi = null;
        });
        _mis.Clear();
    }
}