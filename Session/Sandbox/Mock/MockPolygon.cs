using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using DelaunatorSharp;
using Godot;

public class MockPolygon
{
    public int Id { get; private set; }
    public Vector2 Center { get; private set; }
    public List<LineSegment> BorderSegments { get; private set; }
    public MockPolygon(Vector2 center, List<LineSegment> borderSegs, List<Vector2> riverPoints, List<float> riverWidths,
        int id)
    {
        Center = center;
        Id = id;
    }
}
