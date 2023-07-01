using System;
using System.Collections.Generic;
using System.Linq;
using Godot;

public abstract class Wanderer
{
    public HashSet<MapPolygon> Picked { get; private set; }
    public HashSet<MapPolygon> ValidAdjacent { get; private set; }

    public Wanderer(MapPolygon seed, WandererPicker host)
    {
        Picked = new HashSet<MapPolygon>();
        ValidAdjacent = new HashSet<MapPolygon>();
        host.AddWanderer(this);
        Pick(seed, host);
    }
    public abstract bool MoveAndPick(WandererPicker host);

    protected void Pick(MapPolygon poly, WandererPicker host)
    {
        Picked.Add(poly);
        host.NotTaken.Remove(poly);
        ValidAdjacent.Remove(poly);

        var outside = poly.Neighbors.Entities().Where(Valid).Except(Picked);
        foreach (var p in outside)
        {
            ValidAdjacent.Add(p);
        }
    }
    protected abstract bool Valid(MapPolygon poly);
}
