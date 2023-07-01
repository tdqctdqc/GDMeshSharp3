using Godot;
using System;
using System.Collections.Generic;
using System.Linq;

public class GenPlate 
{
    public int Id { get; private set; }
    public GenCell Seed { get; private set; }
    public MapPolygon GetSeedPoly() => Seed.Seed;
    public GenMass Mass { get; private set; }
    public HashSet<GenCell> Cells { get; private set; }
    public HashSet<GenCell> NeighboringCells { get; private set; }
    public Dictionary<GenCell, int> NeighboringCellsAdjCount { get; private set; }
    public HashSet<GenPlate> Neighbors { get; private set; }
    public Vector2 Center => GetSeedPoly().Center;
    public GenPlate(GenCell seed, int id, GenWriteKey key)
    {
        Id = id;
        Seed = seed;
        Cells = new HashSet<GenCell> {};
        NeighboringCells = new HashSet<GenCell>();
        NeighboringCellsAdjCount = new Dictionary<GenCell, int>();
        AddCell(seed, key);
    }

    public void AddCell(GenCell c, GenWriteKey key)
    {
        Cells.Add(c);
        c.SetPlate(this, key);
        NeighboringCells.Remove(c);
        foreach (var cell in c.Neighbors)
        {
            if (Cells.Contains(cell)) continue;
            NeighboringCells.Add(cell);
            if (NeighboringCellsAdjCount.ContainsKey(cell) == false)
            {
                NeighboringCellsAdjCount.Add(cell, 0);
            }
            NeighboringCellsAdjCount[cell]++;
        }
    }

    public void SetNeighbors()
    {
        Neighbors = NeighboringCells.Select(t => t.Plate).ToHashSet();
    }

    public void SetMass(GenMass c)
    {
        if (Mass != null) throw new Exception();
        Mass = c;
    }
}
