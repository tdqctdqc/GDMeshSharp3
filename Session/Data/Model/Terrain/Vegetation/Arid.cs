
    using System.Collections.Generic;
    using Godot;

    public class Arid : Vegetation
    {
        public Arid() 
            : base(new HashSet<Landform>{LandformManager.Hill, LandformManager.Plain}, 
                .05f, .3f, Colors.YellowGreen.Lightened(.3f), "Arid", true)
        {
        }
    }
