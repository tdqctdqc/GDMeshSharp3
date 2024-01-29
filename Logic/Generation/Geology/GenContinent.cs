using Godot;
using System;
using System.Collections.Generic;
using System.Linq;

public class GenContinent
{
    public int Id { get; private set; }
    public GenMass Seed { get; private set; }
    public HashSet<GenMass> Masses { get; private set; }
    public HashSet<GenMass> NeighboringMasses { get; private set; }
    public Dictionary<GenMass, int> NeighboringMassesAdjCount { get; private set; }
    public HashSet<GenContinent> Neighbors { get; private set; }
    public Vector2 Drift { get; private set; }
    public Vector2 Center { get; private set; }
    public float Altitude { get; private set; }
    public GenContinent(GenMass seed, int id, float altitude)
    {
        Altitude = altitude;
        Center = Vector2.Zero;
        Id = id;
        Seed = seed;
        Masses = new HashSet<GenMass>();
        NeighboringMasses = new HashSet<GenMass>();
        NeighboringMassesAdjCount = new Dictionary<GenMass, int>();
        Drift = Vector2.Left.Rotated(Game.I.Random.RandfRange(0f, 2f * Mathf.Pi));
        AddMass(seed);
    }
    public MapPolygon GetSeedPoly() => Seed.GetSeedPoly();

    public void AddMass(GenMass c)
    {
        Center = (Center * Masses.Count + c.Center) / (Masses.Count + 1);
        Masses.Add(c);
        c.SetContinent(this);
        NeighboringMasses.Remove(c);
        NeighboringMassesAdjCount.Remove(c);
        var border = c.Neighbors.Except(Masses);
        foreach (var nPlate in border)
        {
            NeighboringMasses.Add(nPlate);
            if (NeighboringMassesAdjCount.ContainsKey(nPlate) == false)
            {
                NeighboringMassesAdjCount.Add(nPlate, 0);
            }
            NeighboringMassesAdjCount[nPlate]++;
        }
    }
    public void SetNeighbors()
    {
        Neighbors = NeighboringMasses.Select(t => t.GenContinent).ToHashSet();
    }
    
}
