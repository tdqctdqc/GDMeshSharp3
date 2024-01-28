using System;
using System.Collections.Generic;
using System.Linq;
using Godot;

public class LandformList : ModelList<Landform>
{
    public Landform Peak { get; private set; } 
        = new Peak();
    public Landform Mountain { get; private set; } = new Mountain();
    public Landform Hill { get; private set; } = new Hill();
    public Landform Plain  { get; private set; } = new Landform(nameof(Plain));
    public Landform Sea  { get; private set; } = new Landform(nameof(Sea));
    public Landform River { get; private set; } = new River();
    public Landform Urban { get; private set; } = new Urban();
    public List<Landform> ByPriority { get; private set; }
    public LandformList()
    {
        ByPriority = new List<Landform> { Peak, Mountain, Hill, Sea, Plain, Urban, River };
    }
    
    public Landform GetAtPoint(MapPolygon poly, Vector2 pRel, Data data)
    {
        var close = poly.Neighbors.Items(data).OrderBy(n => (poly.GetOffsetTo(n, data) - pRel).Length());
        var first = close.ElementAt(0);
        var second = close.ElementAt(1);
        var score = poly.GetScore(first, second, pRel, data, 
            p => p.Roughness);
        return ByPriority.First(lf => lf.MinRoughness <= score);
    }
}
