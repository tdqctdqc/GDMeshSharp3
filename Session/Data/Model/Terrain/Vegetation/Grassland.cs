
using System.Collections.Generic;
using Godot;

public class Grassland : Vegetation
{
    public Grassland() 
        : base(new HashSet<Landform>{LandformManager.Hill, LandformManager.Plain}, 
            .2f, 1f, Colors.MediumSeaGreen, "Grassland", true)
    {
    }

}
