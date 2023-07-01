using Godot;
using System;
using System.Collections.Generic;
using System.Linq;

public class GenMass
{
    public int Id { get; private set; }
    public GenContinent GenContinent { get; private set; }
    public GenPlate Seed { get; private set; }
    public HashSet<GenPlate> Plates { get; private set; }
    public HashSet<GenPlate> NeighboringPlates { get; private set; }
    public Dictionary<GenPlate, int> NeighboringPlatesAdjCount { get; private set; }
    public HashSet<GenMass> Neighbors { get; private set; }
    public Vector2 Center { get; private set; }
    public GenMass(GenPlate seed, int id)
    {
        Center = Vector2.Zero;
        Id = id;
        Seed = seed;
        Plates = new HashSet<GenPlate>();
        NeighboringPlates = new HashSet<GenPlate>();
        NeighboringPlatesAdjCount = new Dictionary<GenPlate, int>();
        Neighbors = new HashSet<GenMass>();
        AddPlate(seed);
    }
    public MapPolygon GetSeedPoly() => Seed.GetSeedPoly();
    public void AddPlate(GenPlate c)
    {
        Center = (Center * Plates.Count + c.Center) / (Plates.Count + 1);
        Plates.Add(c);
        c.SetMass(this);
        NeighboringPlates.Remove(c);
        NeighboringPlatesAdjCount.Remove(c);
        var border = c.Neighbors.Except(Plates);
        foreach (var nPlate in border)
        {
            NeighboringPlates.Add(nPlate);
            if (NeighboringPlatesAdjCount.ContainsKey(nPlate) == false)
            {
                NeighboringPlatesAdjCount.Add(nPlate, 0);
            }
            NeighboringPlatesAdjCount[nPlate]++;
        }
    }

    public void SetContinent(GenContinent c)
    {
        GenContinent = c;
    }
    public void SetNeighbors()
    {
        Neighbors = NeighboringPlates.Select(t => t.Mass).ToHashSet();
    }
}
