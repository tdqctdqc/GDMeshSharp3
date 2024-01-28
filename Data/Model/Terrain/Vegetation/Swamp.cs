using System;
using System.Collections.Generic;
using System.Linq;
using Godot;

public class Swamp : Vegetation
{
    public Swamp(LandformList lfs) 
        : base(new HashSet<Landform>{lfs.Plain}, 
            nameof(Swamp))
    {
    }

    public override bool Allowed(MapPolygon p, float moisture, Landform lf, Data data)
    {
        return base.Allowed(p, moisture, lf, data) 
               // && p.Altitude < .6f 
            && p.Roughness < .1f;
    }
}