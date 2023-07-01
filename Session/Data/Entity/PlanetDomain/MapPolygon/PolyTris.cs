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
    public PolyTri[] Tris;
    public byte[] TriNeighbors { get; private set; }

    public static PolyTris Create(List<PolyTri> tris, 
        GenWriteKey key)
    {
        // if (tris.Count == 0) throw new Exception();
        if (tris.Count > 254) throw new Exception("Too many tris");
        

        for (var i = 0; i < tris.Count; i++)
        {
            tris[i].SetIndex((byte)i, key);
        }
        
        
        var ts = new PolyTris(tris.ToArray(), new byte[0]);
        
        // ts.SetNeighbors(key);
        
        return ts;
    }

    [SerializationConstructor] private PolyTris(PolyTri[] tris,
        byte[] triNeighbors)
    {
        Tris = tris;
        TriNeighbors = triNeighbors;
    }

    public PolyTri GetAtPoint(Vector2 point, Data data)
    {
        return Tris.FirstOrDefault(t => t.ContainsPoint(point));
    }

    public void SetNeighbors(MapPolygon poly, GenWriteKey key)
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