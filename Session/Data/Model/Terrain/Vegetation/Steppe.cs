using System;
using System.Collections.Generic;
using System.Linq;
using Godot;

public class Steppe : Vegetation
{
    public Steppe() 
        : base(new HashSet<Landform>{LandformManager.Hill, LandformManager.Plain}, 
            .1f, .6f, Colors.PaleGreen, "Steppe", true)
    {
    }
}
