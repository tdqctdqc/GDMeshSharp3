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
        return ts;
    }

    [SerializationConstructor] private PolyTris(PolyTri[] tris,
        byte[] triNeighbors)
    {
        Tris = tris;
    }

    public PolyTri GetAtPoint(Vector2 point, Data data)
    {
        return Tris.FirstOrDefault(t => t.ContainsPoint(point));
    }
}