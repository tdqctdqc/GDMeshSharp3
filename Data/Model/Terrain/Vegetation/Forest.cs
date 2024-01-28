
using System.Collections.Generic;
using Godot;

public class Forest : Vegetation
{
    public Forest(LandformList lfs) : base(new HashSet<Landform>{lfs.Hill, lfs.Plain}, 
        nameof(Forest))
    {
        
    }
}
