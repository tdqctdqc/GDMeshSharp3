using System;
using System.Collections.Generic;
using System.Linq;
using Godot;

public class Swamp : Vegetation
{
    public Swamp() 
    {
    }

    public override bool Allowed(MapPolygon p, float moisture, 
        Landform lf, Data data)
    {
        return base.Allowed(p, moisture, lf, data) 
            && lf.MinRoughness < .1f;
    }
}