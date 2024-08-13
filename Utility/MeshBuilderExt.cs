
using System;
using System.Collections.Generic;
using System.Linq;
using Godot;

public static class MeshBuilderExt
{
    public static void DrawCellPath(this MeshBuilder mb,
        Vector2 relTo, List<Cell> path,
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
    
    
    public static void DrawPolygonOutline(this MeshBuilder mb,
        Vector2[] boundaryPoints, float thickness, Color color)
    {
        for (var i = 0; i < boundaryPoints.Length; i++)
        {
            var from = boundaryPoints[i];
            var to = boundaryPoints.Modulo(i + 1);
            mb.AddLine(from, to, color, thickness);
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

    public static void DrawCellRel(this MeshBuilder mb,
        Cell cell, Vector2 relTo, Color color, Data d)
    {
        mb.DrawPolygonRel(cell.AbsBoundary(d).ToArray(),
            color, relTo, d);
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
            DrawFrontFace(mb, face, color, thickness, relTo, d);
        }
    }

    public static void DrawFrontFace(this MeshBuilder mb,
        FrontFace face,
        Color color, 
        float thickness,
        Vector2 relTo, Data d)
    {
        var (start, end) = (face.GetLeftNexusPoint(d), face.GetRightNexusPoint(d));
        mb.AddArrowRel(start, end, thickness, color, relTo, d);
    }
}