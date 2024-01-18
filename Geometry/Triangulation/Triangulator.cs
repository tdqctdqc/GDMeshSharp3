using Godot;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using DelaunatorSharp;
using MIConvexHull;
using Poly2Tri;


public static class Triangulator
{
    
    
    public static List<Triangle> TriangulatePoints(List<Vector2> points)
    {
        var d = new Delaunator(points.Select(p => new DelaunatorPoint(p)).ToArray());
        var tris = new List<Triangle>();
        for (int i = 0; i < d.Triangles.Length; i+=3)
        {
            var triIndex = i / 3;
            var pointId1 = d.Triangles[i];
            var dPoint1 = d.Points[pointId1];
            
            var pointId2 = d.Triangles[i + 1];
            var dPoint2 = d.Points[pointId2];
            
            var pointId3 = d.Triangles[i + 2];
            var dPoint3 = d.Points[pointId3];
            var adj = d.TrianglesAdjacentToTriangle(triIndex);
            
            tris.Add(new Triangle(dPoint1.GetV2(), dPoint2.GetV2(), dPoint3.GetV2()));
        }
        return tris;
    }
    
    
    public class DelaunatorPoint : IPoint
    {
        public double X {get; set;}
        public double Y {get; set;}

        public DelaunatorPoint(Vector2 v)
        {
            X = (int)v.X;
            Y = (int)v.Y;
        }
    }
}
public static class IPointExt
{
    public static IPoint GetIPoint(this Vector2 v)
    {
        return new Triangulator.DelaunatorPoint(v);
    }
    public static Vector2 GetV2(this IPoint p)
    {
        return new Vector2((float) p.X, (float) p.Y);
    }
    public static Vector2 GetIntV2(this IPoint p)
    {
        return new Vector2(Mathf.FloorToInt((float)p.X), Mathf.FloorToInt((float)p.Y));
    }
}