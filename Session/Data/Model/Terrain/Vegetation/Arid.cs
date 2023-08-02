
    using System.Collections.Generic;
    using Godot;

    public class Arid : Vegetation
    {
        public Arid(LandformList lfs) 
            : base(new HashSet<Landform>{lfs.Hill, lfs.Plain}, 
                .05f, .3f, Colors.YellowGreen.Lightened(.3f), "Arid", true)
        {
        }
    }
