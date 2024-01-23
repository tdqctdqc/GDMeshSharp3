
    using System.Collections.Generic;
    using Godot;

    public class Arid : Vegetation
    {
        public Arid(LandformList lfs) 
            : base(new HashSet<Landform>{lfs.Hill, lfs.Plain}, 
                .1f, .3f, 
                1f,
                Colors.YellowGreen.Lightened(.3f), "Arid")
        {
        }
    }
