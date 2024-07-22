using System;
using System.Collections.Generic;
using System.Linq;
using Godot;

public class VegetationList : ModelPredefs<Vegetation>
{
    public Vegetation Swamp { get; private set; }
        = new Swamp();
    public Vegetation Forest { get; private set; }
        = new Vegetation();
    public Vegetation Grassland { get; private set; }
        = new Vegetation();
    public Vegetation Arid { get; private set; }
        = new Vegetation();
    public Vegetation Steppe { get; private set; }
        = new Vegetation();
    public Vegetation Desert { get; private set; }
        = new Vegetation();
    public Vegetation Barren { get; private set; }
        = new Vegetation();
    public Vegetation Jungle { get; private set; }
        = new Jungle();
    public Vegetation Tundra { get; private set; }
        = new Tundra();
    public List<Vegetation> ByPriority { get; private set; }

    public VegetationList(LandformList lfs)
    {
        ByPriority = new List<Vegetation> { Swamp, Jungle, Forest, Tundra,
            Grassland, Steppe, Arid, Desert, Barren };
    }
    public Vegetation GetAtPoint(MapPolygon poly, Vector2 pRel, Landform lf, Data data)
    {
        var close = poly.Neighbors.Entities(data).OrderBy(n => (poly.GetOffsetTo(n, data) - pRel).Length());
        var first = close.ElementAt(0);
        var second = close.ElementAt(1);
        var score = poly.GetScore(first, second, pRel, data, p => p.Moisture);
        try
        {
            return ByPriority.First(v => v.Allowed(poly, score, lf, data));
        }
        catch (Exception e)
        {
            GD.Print($"cant find veg for lf {lf.Name} and moisture score {score}");
            throw;
        }
    }
}
