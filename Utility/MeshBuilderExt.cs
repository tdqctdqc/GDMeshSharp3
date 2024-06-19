
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
            mb.AddSquare(relTo.Offset(cell.GetCenter(), d),
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

        foreach (var kvp in 
                 MilAiUtil.GetGroupLineAssignments(seg.Alliance, seg.Groups,
                     seg.Frontline.Faces, d))
        {
            var held = kvp.Value;
            var group = kvp.Key;
            for (var i = 0; i < held.Count; i++)
            {
                var face = seg.Frontline.Faces[i];
                var native = face.GetNative(d);
                var foreign = face.GetForeign(d);
                mb.AddArrow(relTo.Offset(native.GetCenter(),d),
                    relTo.Offset(foreign.GetCenter(), d),
                    markerSize / 5f, group.Color);
            }
            foreach (var cell in held)
            {
                foreach (var neighbor in cell.GetNeighbors(d))
                {
                    if (cell.Id < neighbor.Id) continue;
                    if (held.Contains(neighbor) == false) continue;
                    mb.AddLine(relTo.Offset(cell.GetCenter(), d),
                        relTo.Offset(neighbor.GetCenter(), d),
                        group.Color, markerSize / 2f);
                }
            }
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
}