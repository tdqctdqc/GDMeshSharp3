
using System.Collections.Generic;
using Godot;

public class Tundra : Vegetation
{
    public static float MinDistFromEquatorRatio { get; private set; }  = .45f;
    public Tundra() 
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