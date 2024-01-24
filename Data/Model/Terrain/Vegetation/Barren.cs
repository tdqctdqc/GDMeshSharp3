
using System.Collections.Generic;
using Godot;

public class Barren : Vegetation
{
    public Barren(LandformList lfs) 
        : base(new HashSet<Landform>{lfs.Mountain, lfs.Peak, lfs.Urban, lfs.River, lfs.Sea}, 
            0f, 0f, 
            1f, Colors.Transparent, "Barren")
    {
    }
}