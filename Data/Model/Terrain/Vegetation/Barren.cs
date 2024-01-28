
using System.Collections.Generic;
using Godot;

public class Barren : Vegetation
{
    public Barren(LandformList lfs) 
        : base(new HashSet<Landform>{lfs.Mountain, lfs.Peak, lfs.Urban, lfs.River, lfs.Sea}, 
            nameof(Barren))
    {
    }
}