using System;
using System.Collections.Generic;
using System.Linq;
using Godot;

public class Steppe : Vegetation
{
    public Steppe(LandformList lfs) 
        : base(new HashSet<Landform>{lfs.Hill, lfs.Plain}, 
            nameof(Steppe))
    {
    }
}
