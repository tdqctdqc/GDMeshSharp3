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

    private static Vector2 RelPos(Vector2 pos)
    {
        return Game.I.Client.Cam().GetMapPosInGlobalSpace(pos);
    }

    public void DrawFrontSegment(FrontSegmentAssignment seg, Data d)
    {
        var mb = new MeshBuilder();
        for (var i = 0; i < seg.LineWaypointIds.Count - 1; i++)
        {
            var from = MilitaryDomain.GetTacWaypoint(seg.LineWaypointIds[i], d);
            var to = MilitaryDomain.GetTacWaypoint(seg.LineWaypointIds[i + 1], d);
            mb.AddLine(RelPos(from.Pos), RelPos(to.Pos), Colors.Red, 3f);
        }
        var groups = seg.Groups(d);
        foreach (var group in groups)
        {
            foreach (var unit in group.Units.Items(d))
            {
                var pos = unit.Position;
                mb.AddSquare(RelPos(pos), 10f, Colors.Red);
            }
        }

        var rear = seg.GetRear(d, 3);
        for (var i = 0; i < rear.Count; i++)
        {
            var ring = rear[i];
            foreach (var rWp in ring)
            {
                mb.AddSquare(RelPos(rWp.Pos), 10f, ColorsExt.GetRainbowColor(i));
            }
        }
        mb.AddSquare(RelPos(MilitaryDomain.GetTacWaypoint(seg.RallyWaypointId, d).Pos),
            20f, Colors.Blue);
        TakeFromMeshBuilder(mb);
    }
    public void DrawPoly(Data data, PolyTriPosition pos)
    {
        Visible = true;
        Clear();
        var poly = pos.Poly(data);
        var pt = pos.Tri(data);

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

    private static void DrawBordersSimple(MapPolygon poly, MeshBuilder mb, Data data)
    {
        var edgeBorders = poly.GetOrderedBoundarySegs(data)
            .Select(s => s.Translate(RelPos(poly.Center))).ToList();
        mb.AddLines(edgeBorders, 2f, Colors.Black);
    }
    private static void DrawBoundarySegments(MapPolygon poly, MeshBuilder mb, Data data)
    {
        var lines = poly.GetOrderedBoundarySegs(data)
            .Select(s => s.Translate(RelPos(poly.Center))).ToList();
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
            var a = RelPos(inscribed.A);
            var b = RelPos(inscribed.B);
            var c = RelPos(inscribed.C);

            mb.AddArrow(a, b, 1f, col);
            mb.AddArrow(b, c, 1f, col);
            mb.AddArrow(c, a, 1f, col);
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