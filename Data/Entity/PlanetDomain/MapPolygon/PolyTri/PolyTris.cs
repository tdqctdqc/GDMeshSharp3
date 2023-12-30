using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Godot;
using MessagePack;

public class PolyTris
{
    public PolyTri this[int i] => Tris[i];
    public PolyTri[] Tris { get; private set; }
    public byte[] TriNeighbors { get; private set; }

    public static PolyTris Create(IEnumerable<PolyTri> tris, 
        GenWriteKey key)
    {
        // if (tris.Count == 0) throw new Exception();
        if (tris.Count() > 254) throw new Exception("Too many tris");

        var trisA = tris.Where(t => float.IsNaN(t.GetArea()) == false).ToArray();
        for (var i = 0; i < trisA.Length; i++)
        {
            trisA[i].SetIndex((byte)i, key);
        }
        var ts = new PolyTris(trisA, new byte[0]);
        
        ts.SetNeighbors(key);
        return ts;
    }

    [SerializationConstructor] private PolyTris(PolyTri[] tris,
        byte[] triNeighbors)
    {
        Tris = tris;
        TriNeighbors = triNeighbors;
    }

    public PolyTri GetAtPoint(Vector2 pointRel, Data data)
    {
        var pt = Tris.FirstOrDefault(t => 
            t.ContainsPoint(pointRel));
        if (pt == null)
        {
            var close = Tris.MinBy(t => t.GetDistFromEdge(pointRel));
            var dist = close.GetDistFromEdge(pointRel);
            GD.Print($"couldnt find pt at rel point {pointRel}," +
                     $"found tri at dist {dist}");
            return close;
        }
        return pt;
    }
    
    public void SetNeighbors(GenWriteKey key)
    {
        var points = Tris.Select(t => t.A)
            .Union(Tris.Select(t => t.B))
            .Union(Tris.Select(t => t.C))
            .Distinct()
            .ToDictionary(v => v, v => new LinkedList<byte>());


        for (var i = 0; i < Tris.Length; i++)
        {
            var t = Tris[i];
            points[t.A].AddLast(t.Index);
            points[t.B].AddLast(t.Index);
            points[t.C].AddLast(t.Index);
        }


        var triNativeNeighbors = new List<byte>();
        var hash = new HashSet<byte>();
        int iter = 0;
        for (var i = 0; i < Tris.Length; i++)
        {
            hash.Clear();
            var tri = Tris[i];
            tri.SetNeighborStart(iter, key);
            int nCount = 0;
            addNeighborsOfPoint(tri.A);
            addNeighborsOfPoint(tri.B);
            addNeighborsOfPoint(tri.C);

            void addNeighborsOfPoint(Vector2 point)
            {
                foreach (var j in points[point])
                {
                    if (j != tri.Index && hash.Contains(j) == false)
                    {
                        hash.Add(j);
                        triNativeNeighbors.Add(j);
                        iter++;
                        nCount++;
                    }
                }
            }

            if (nCount > 254) throw new Exception("too many neighbors"); 
            tri.SetNeighborCount((byte)nCount, key);
        }

        TriNeighbors = triNativeNeighbors.ToArray();
    }
}