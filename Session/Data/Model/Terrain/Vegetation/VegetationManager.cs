using System;
using System.Collections.Generic;
using System.Linq;
using Godot;

public class VegetationManager : TerrainAspectManager<Vegetation>
{
    public static Vegetation Swamp = new Swamp();

    public static Vegetation Forest = new Forest();
    
    public static Vegetation Grassland = new Grassland();

    public static Vegetation Arid = new Arid();
    public static Vegetation Steppe = new Steppe();
    
    public static Vegetation Desert = new Vegetation(
        new HashSet<Landform>{LandformManager.Hill, LandformManager.Plain}, 
        0f, .0f, Colors.Tan, "Desert", true);
    
    public static Vegetation Barren = new Vegetation(
        new HashSet<Landform>{LandformManager.Mountain, LandformManager.Peak, LandformManager.Urban, LandformManager.River, LandformManager.Sea}, 
        0f, 0f, Colors.Transparent, "Barren", true);


    public VegetationManager() 
        : base(Barren, Barren, 
            new List<Vegetation>{Swamp, Forest, Grassland, Steppe, Arid, Desert, Barren})
    {
        
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