using Godot;
using System;
using System.Collections.Generic;
using System.Linq;

public sealed class GenCell : IGraphNode<GenCell>
{
    private GenCell _element;
    public MapPolygon Seed { get; private set; }
    public GenPlate Plate { get; private set; }
    public HashSet<MapPolygon> Polys { get; private set; }
    public HashSet<MapPolygon> NeighboringPolyGeos { get; private set; }

    GenCell IReadOnlyGraphNode<GenCell>.Element => _element;

    IReadOnlyCollection<GenCell> IReadOnlyGraphNode<GenCell>.Neighbors => Neighbors;
    bool IReadOnlyGraphNode<GenCell>.HasNeighbor(GenCell neighbor) => Neighbors.Contains(neighbor);
    public HashSet<GenCell> Neighbors { get; private set; }
    public Vector2 Center { get; private set; }
    private Dictionary<MapPolygon, GenCell> _polyCells { get; }

    public GenCell(MapPolygon seed, GenWriteKey key, Dictionary<MapPolygon, GenCell> polyCells, GenData data)
    {
        _polyCells = polyCells;
        Center = Vector2.Zero;
        Seed = seed;
        Polys = new HashSet<MapPolygon>();
        NeighboringPolyGeos = new HashSet<MapPolygon>();
        Neighbors = new HashSet<GenCell>();
        AddPolygon(seed, key);
    }

    public void SetPlate(GenPlate plate, GenWriteKey key)
    {
        Plate = plate;
    }
    public void AddPolygon(MapPolygon p, GenWriteKey key)
    {
        Center = (Center * Polys.Count + p.Center) / (Polys.Count + 1);
        Polys.Add(p);
        _polyCells[p] = this;
        NeighboringPolyGeos.Remove(p);
        foreach (var n in p.Neighbors.Items(key.Data))
        {
            if(Polys.Contains(n) == false) NeighboringPolyGeos.Add(n);
        }
    }


    public void SetNeighbors(GenWriteKey key)
    {
        foreach (var p in NeighboringPolyGeos)
        {
            if (key.GenData.GenAuxData.PolyGenCells.ContainsKey(p) == false)
            {
                throw new Exception($"No aux data for cell at " + p.Center);
            }
        }
        Neighbors = NeighboringPolyGeos
            .Select(t => key.GenData.GenAuxData.PolyGenCells[t]).ToHashSet();
    }
}
