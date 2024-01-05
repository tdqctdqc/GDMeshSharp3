
using System.Collections.Generic;
using Godot;

public class Jungle : Vegetation
{
    public float MaxDistRatioFromEquator { get; private set; } = .15f;
    public Jungle(LandformList lfs) 
        : base(new HashSet<Landform>{lfs.Hill, lfs.Plain}, 
            .5f, .4f,
            .5f,
            new Color("#4bcf0e").Darkened(.25f), nameof(Jungle))
    {
    }
    public override bool Allowed(MapPolygon p, float moisture, Landform lf, Data data)
    {
        var mapHeight = data.Planet.Height;
        var distFromEquatorRatio = Mathf.Abs((.5f * mapHeight - p.Center.Y) / mapHeight);
        return base.Allowed(p, moisture, lf, data) 
               && p.DistFromEquatorRatio(data) < MaxDistRatioFromEquator;
    }
}