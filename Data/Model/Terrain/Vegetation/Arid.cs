
    using System.Collections.Generic;
    using Godot;

    public class Arid : Vegetation
    {
        public Arid(LandformList lfs) 
            : base(new HashSet<Landform>{lfs.Hill, lfs.Plain}, 
                nameof(Arid))
        {
        }
    }
