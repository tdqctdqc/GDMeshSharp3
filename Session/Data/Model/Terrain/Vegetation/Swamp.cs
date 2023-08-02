using System;
using System.Collections.Generic;
using System.Linq;
using Godot;

public class Swamp : Vegetation
{
    public Swamp(LandformList lfs) 
        : base(new HashSet<Landform>{lfs.Plain}, .7f, .25f, Colors.DarkOliveGreen, "Swamp",
            true)
    {
    }

    public override bool Allowed(MapPolygon p, float moisture, Landform lf, Data data)
    {
        return base.Allowed(p, moisture, lf, data) 
               // && p.Altitude < .6f 
            && p.Roughness < .15f;
    }
}