using System;
using System.Collections.Generic;
using System.Linq;
using Godot;

public class MapChunk
{
    public Vector2 Coords { get; private set; }
    public HashSet<MapPolygon> Polys { get; private set; }
    public HashSet<Cell> Cells { get; private set; }
    public HashSet<MapPolygon> Bordering { get; private set; }
    public MapPolygon RelTo { get; private set; }
    public Color Color { get; private set; }
    
    public MapChunk(IEnumerable<MapPolygon> polys, 
        IEnumerable<Cell> cells,
        Vector2 coords, Data d)
    {
        Coords = coords;
        Polys = polys.ToHashSet();
        Cells = cells.ToHashSet();
        Bordering = Polys.SelectMany(p => p.Neighbors.Items(d))
            .Where(n => Polys.Contains(n) == false).ToHashSet();
        RelTo = polys.First();
        Color = ColorsExt.GetRandomColor();
    }
}