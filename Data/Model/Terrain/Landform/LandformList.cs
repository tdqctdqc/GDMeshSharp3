using System;
using System.Collections.Generic;
using System.Linq;
using Godot;
using VoronoiSandbox;

public class LandformList : ModelPredefs<Landform>
{
    public Landform Peak { get; private set; } 
        = new ();
    public Landform Mountain { get; private set; } 
        = new ();
    public Landform Hill { get; private set; } 
        = new ();
    public Landform Plain  { get; private set; } 
        = new ();
    public Landform Sea  { get; private set; } 
        = new ();
    public Landform River { get; private set; } 
        = new ();
    public Landform Urban { get; private set; } 
        = new ();
    public List<Landform> ByPriority { get; private set; }
    public LandformList()
    {
        ByPriority = new List<Landform> { Peak, Mountain, Hill, Sea, Plain, Urban, River };
    }
    
    public Landform GetAtPoint(MapPolygon poly,
        PreCell pre,
        Vector2 pRel, Data data)
    {
        return ByPriority.First(lf => lf.MinRoughness <= pre.Roughness);
    }
}
