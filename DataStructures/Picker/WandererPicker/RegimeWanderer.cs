using System;
using System.Collections.Generic;
using System.Linq;
using Godot;

public class RegimeWanderer : Wanderer
{
    public Regime Regime { get; private set; }
    public RegimeWanderer(Regime regime, MapPolygon seed, WandererPicker host, int numToPick) 
        : base(seed, host, numToPick)
    {
        Regime = regime;
    }
    
    public override bool MoveAndPick(WandererPicker host)
    {
        if (ValidAdjacent.Any(host.NotTaken.Contains) == false) return false;

        if (Picked.Count < 4)
        {
            foreach (var a in ValidAdjacent)
            {
                if (host.NotTaken.Contains(a) == false) continue;
                Add(a, host);
                return true;
            }
            return false;
        }

        var aCount = 0;
        MapPolygon pick = null;
        var found = false;
        
        foreach (var a in ValidAdjacent)
        {
            if (host.NotTaken.Contains(a) == false) continue;
            if (a.Neighbors.Entities().Where(n => Picked.Contains(n)).Count() <= 1) continue;
            var count = a.Neighbors.Entities().Where(n => Picked.Contains(n)).Count();
            if (count > aCount)
            {
                pick = a;
                aCount = count;
                found = true;
            }
        }
        if (found)
        {
            Add(pick, host);
            return true;
        }

        return false;
    }


    protected override bool Valid(MapPolygon poly)
    {
        return poly.IsLand;
    }
}
