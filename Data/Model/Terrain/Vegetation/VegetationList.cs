using System;
using System.Collections.Generic;
using System.Linq;
using Godot;

public class VegetationList : ModelList<Vegetation>
{
    public Vegetation Swamp { get; private set; }
    public Vegetation Forest { get; private set; }
    public Vegetation Grassland { get; private set; }
    public Vegetation Arid { get; private set; }
    public Vegetation Steppe { get; private set; }
    public Vegetation Desert { get; private set; }
    public Vegetation Barren { get; private set; }
    public Vegetation Jungle { get; private set; }
    public Tundra Tundra { get; private set; }
    public List<Vegetation> ByPriority { get; private set; }

    public VegetationList(LandformList lfs)
    {
        Swamp = new Swamp(lfs);
        Forest = new Forest(lfs);
        Grassland = new Grassland(lfs);
        Arid = new Arid(lfs);
        Steppe = new Steppe(lfs);
        Desert = new Desert(lfs);
        Barren = new Barren(lfs);
        Jungle = new Jungle(lfs);
        Tundra = new Tundra(lfs);
        
        ByPriority = new List<Vegetation> { Swamp, Jungle, Forest, Tundra,
            Grassland, Steppe, Arid, Desert, Barren };
    }
    public Vegetation GetAtPoint(MapPolygon poly, Vector2 pRel, Landform lf, Data data)
    {
        var close = poly.Neighbors.Items(data).OrderBy(n => (poly.GetOffsetTo(n, data) - pRel).Length());
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
