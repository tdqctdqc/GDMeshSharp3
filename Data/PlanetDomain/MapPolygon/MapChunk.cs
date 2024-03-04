using System;
using System.Collections.Generic;
using System.Linq;
using Godot;

public class MapChunk
{
    public static int ChunkDim = 2000;
    public Vector2 Coords { get; private set; }
    public HashSet<MapPolygon> Polys { get; private set; }
    public List<Cell> Cells { get; private set; }
    public Dictionary<Cell, int> CellTriCounts { get; private set; }
    public Vector2[] CellTriVertices { get; private set; }
    public MapPolygon RelTo { get; private set; }
    public Color Color { get; private set; }
    
    public MapChunk(IEnumerable<MapPolygon> polys, 
        IEnumerable<Cell> cells,
        Vector2 coords, Data d)
    {
        Coords = coords;
        Polys = polys.ToHashSet();
        Cells = cells.ToList();
        RelTo = polys.First();
        Color = ColorsExt.GetRandomColor();


        var vertices = new List<Vector2>();
        CellTriCounts = new Dictionary<Cell, int>();
        for (var i = 0; i < Cells.Count; i++)
        {
            var cell = Cells[i];
            var tris = Geometry2D.TriangulatePolygon(cell.RelBoundary);
            for (var j = 0; j < tris.Length; j+=3)
            {
                var a = cell.RelBoundary[tris[j]];
                var b = cell.RelBoundary[tris[j+1]];
                var c = cell.RelBoundary[tris[j+2]];
                vertices.Add(RelTo.Center.Offset(a + cell.RelTo, d));
                vertices.Add(RelTo.Center.Offset(b + cell.RelTo, d));
                vertices.Add(RelTo.Center.Offset(c + cell.RelTo, d));
            }

            CellTriCounts.Add(cell, tris.Length / 3);
        }

        CellTriVertices = vertices.ToArray();
    }
}