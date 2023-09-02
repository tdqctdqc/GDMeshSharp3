using System;
using System.Collections.Generic;
using System.Linq;
using Godot;

public partial class PolyHighlighter : Node2D
{
    private List<MeshInstance2D> _mis;

    public PolyHighlighter(Data data)
    {
        Game.I.Client.UiRequests.MouseOver
            .SubscribeForNode(pos => Draw(data, pos), this);
        _mis = new List<MeshInstance2D>();
    }
    private PolyHighlighter()
    {
    }
    public enum Modes
    {
        Simple,
        Complex
    }
    public void Draw(Data data, PolyTriPosition pos)
    {
        Visible = true;
        Clear();
        var poly = pos.Poly(data);
        var pt = pos.Tri(data);
        Move(poly);
        var mb = new MeshBuilder();
        
        var mode = Game.I.Client.Settings.PolyHighlightMode.Value;
        if (mode == Modes.Simple)
        {
            DrawSimple(data, poly, pt, mb);
        }
        else if (mode == Modes.Complex)
        {
            DrawComplex(data, pos, poly, pt, mb);
        }
        else throw new Exception();
        
        TakeFromMeshBuilder(mb);
    }

    private void DrawSimple(Data data, MapPolygon poly, PolyTri pt, MeshBuilder mb)
    {
        DrawBordersSimple(poly, mb, data);
        // DrawAssocWaypoints(poly, mb, data);
        // DrawnNeighborBordersSimple(poly, mb, data);
    }

    private void DrawAssocWaypoints(MapPolygon poly, MeshBuilder mb, Data data)
    {
        var wps = data.Planet.Nav.GetPolyAssocWaypoints(poly, data);
        foreach (var waypoint in wps)
        {
            var offset = poly.GetOffsetTo(waypoint.Pos, data);
            mb.AddCircle(offset, 20f, 6, Colors.Purple);
        }
    }
    private void DrawNavPathsToNeighbors(MapPolygon poly, MeshBuilder mb, Data data)
    {
        var nav = data.Planet.Nav;
        var polyWp = nav.GetPolyCenterWaypoint(poly);
        foreach (var nPoly in poly.Neighbors.Items(data))
        {
            var nPolyWp = nav.GetPolyCenterWaypoint(nPoly);
            var path = PathFinder.FindNavPath(poly, nPoly, data);
            for (var i = 0; i < path.Count - 1; i++)
            {
                var from = path[i];
                var to = path[i + 1];
                mb.AddLine(poly.GetOffsetTo(from.Pos, data), poly.GetOffsetTo(to.Pos, data), 
                    Colors.Red, 10f);
            }
        }
    }
    private static void DrawComplex(Data data, PolyTriPosition pos, MapPolygon poly, PolyTri pt, MeshBuilder mb)
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
    private void Move(MapPolygon poly)
    {
        Position = Game.I.Client.Cam().GetMapPosInGlobalSpace(poly.Center);
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