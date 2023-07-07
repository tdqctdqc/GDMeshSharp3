using System;
using System.Collections.Generic;
using System.Linq;
using Godot;

public class LandformManager : TerrainAspectManager<Landform>
{
    public static Landform Peak { get; private set; } 
        = new Peak();
    public static Landform Mountain { get; private set; } = new Mountain();
    public static Landform Hill { get; private set; } = new Hill();
    public static Landform Plain  { get; private set; } = new Landform("Plain", 0f, 1f, Colors.SaddleBrown, false);
    public static Landform Sea  { get; private set; } = new Landform("Sea", Mathf.Inf, 0f, 
        Colors.DodgerBlue.Darkened(.2f), true);
    public static Landform River { get; private set; } = new River();
    public static Landform Urban { get; private set; } = new Urban();
    public LandformManager()
        : base(Sea, Plain, new List<Landform>{Urban, River, Peak, Mountain, Hill, Sea, Plain})
    {
        
    }
    
    public Landform GetAtPoint(MapPolygon poly, Vector2 pRel, Data data)
    {
        var close = poly.Neighbors.Entities(data).OrderBy(n => (poly.GetOffsetTo(n, data) - pRel).Length());
        var first = close.ElementAt(0);
        var second = close.ElementAt(1);
        var score = poly.GetScore(first, second, pRel, data, 
            p => p.Roughness);
        return ByPriority.First(lf => lf.MinRoughness <= score);
    }
}