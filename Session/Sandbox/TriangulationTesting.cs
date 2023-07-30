
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Godot;
using Poly2Tri;
public class TriangulationTesting
{

    public void Run()
    {
        // var mult = 100f;
        // var p0_0 = new Vector2(0f * mult, 0f * mult);
        // var p1_0 = new Vector2(1f * mult, 0f * mult);
        // var p1_1 = new Vector2(1f * mult, 1f * mult);
        // var p0_1 = new Vector2(0f * mult, 1f * mult);
        // var p2_0 = new Vector2(2f * mult, 0f * mult);
        // var p2_1= new Vector2(2f * mult, 1f * mult);
        // var p2_2= new Vector2(2f * mult, 2f * mult);
        // var p1_2= new Vector2(1f * mult, 2f * mult);
        // var p3_1= new Vector2(3f * mult, 1f * mult);
        // var p3_0= new Vector2(3f * mult, 0f * mult);
        //
        // var segs = new List<LineSegment>
        // {
        //     new LineSegment(p0_0, p1_0),
        //     new LineSegment(p1_0, p2_0),
        //     new LineSegment(p2_0, p3_0),
        //     new LineSegment(p3_0, p3_1),
        //     new LineSegment(p3_1, p2_1),
        //     new LineSegment(p2_1, p2_2),
        //     new LineSegment(p2_2, p1_2),
        //     new LineSegment(p1_2, p1_1),
        //     new LineSegment(p1_1, p0_1),
        //     new LineSegment(p0_1, p0_0),
        // };
        //
        // var squareSegs = new List<LineSegment>
        // {
        //     new LineSegment(p0_0, p1_0),
        //     new LineSegment(p1_0, p1_1),
        //     new LineSegment(p1_1, p0_1),
        //     new LineSegment(p0_1, p0_0),
        // };
        //
        // var interiorPs = new HashSet<Vector2>
        // {
        //     new Vector2(.5f * mult, .5f * mult),
        //     new Vector2(1.5f * mult, .5f * mult),
        //     new Vector2(2.5f * mult, .5f * mult),
        //     new Vector2(1.5f * mult, 1.5f * mult),
        // };
        // var sw = new Stopwatch();
        // sw.Start();
        // List<Triangle> tris = segs.Triangulate();
        // sw.Stop();
        // for (int i = 0; i < 10; i++)
        // {
        //     sw.Reset();
        //     sw.Start();
        //     tris = segs.Triangulate();
        //     sw.Stop();
        //     GD.Print("triangulation took " + sw.Elapsed.TotalMilliseconds+"ms");
        // }
        //
        //
        // tris.ForEach(t => _client.DrawTri(t));
    }

    
}
