using System;
using System.Collections.Generic;
using System.Linq;
using Godot;

public abstract class Wanderer
{
    public HashSet<MapPolygon> Picked { get; private set; }
    public HashSet<MapPolygon> ValidAdjacent { get; private set; }
    public int NumToPick { get; private set; }

    public Wanderer(MapPolygon seed, WandererPicker host, int numToPick, Data data)
    {
        NumToPick = numToPick;
        Picked = new HashSet<MapPolygon>();
        ValidAdjacent = new HashSet<MapPolygon>();
        host.AddWanderer(this);
        Add(seed, host, data);
    }

    public bool Pick(WandererPicker host, Data data)
    {
        for (var i = 0; i < NumToPick; i++)
        {
            var open = MoveAndPick(host, data);
            if (open == false) return false;
        }

        return true;
    }
    public abstract bool MoveAndPick(WandererPicker host, Data data);

    protected void Add(MapPolygon poly, WandererPicker host, Data data)
    {
        
        Picked.Add(poly);
        host.NotTaken.Remove(poly);
        ValidAdjacent.Remove(poly);

        var outside = poly.Neighbors.Entities(data).Where(Valid).Except(Picked);
        foreach (var p in outside)
        {
            ValidAdjacent.Add(p);
        }
    }
    protected abstract bool Valid(MapPolygon poly);
}
