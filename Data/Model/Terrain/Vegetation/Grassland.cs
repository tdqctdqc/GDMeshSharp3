
using System.Collections.Generic;
using Godot;

public class Grassland : Vegetation
{
    public Grassland(LandformList lfs) 
        : base(new HashSet<Landform>{lfs.Hill, lfs.Plain}, 
            nameof(Grassland))
    {
    }

}
