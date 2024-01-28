
using System.Collections.Generic;
using Godot;

public class Tundra : Vegetation
{
    public float MinDistFromEquatorRatio { get; private set; }  = .45f;
    public Tundra(LandformList lfs) 
        : base(new HashSet<Landform>{lfs.Hill, lfs.Plain},
            nameof(Tundra))
    {
    }
    public override bool Allowed(MapPolygon p, float moisture, Landform lf, Data data)
    {
        var mapHeight = data.Planet.Height;
        var distFromEquatorRatio = Mathf.Abs((.5f * mapHeight - p.Center.Y) / mapHeight);
        return base.Allowed(p, moisture, lf, data) 
               && p.DistFromEquatorRatio(data) > MinDistFromEquatorRatio;
    }
}