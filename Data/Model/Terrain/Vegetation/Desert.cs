
using System.Collections.Generic;
using Godot;

public class Desert : Vegetation
{
    public Desert(LandformList lfs) 
        : base(new HashSet<Landform>{lfs.Hill, lfs.Plain}, 
            0f, .0f, 
            1f, Colors.Tan, "Desert")
    {
    }
}