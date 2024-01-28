using System;
using System.Collections.Generic;
using System.Linq;
using Godot;
using Microsoft.FSharp.Data.UnitSystems.SI.UnitNames;

public class Vegetation : TerrainAspect
{
    public HashSet<Landform> AllowedLandforms { get; private set; }
    public float MinMoisture { get; private set; }
    public float FertilityMod { get; private set; }
    public float MovementCostMult { get; private set; }

    public Vegetation(HashSet<Landform> allowedLandforms,
        string name)
    {
        var res = GD.Load<VegetationRes>($"Data/Model/Terrain/Vegetation/{name}.tres");
        FertilityMod = res.FertilityMod;
        AllowedLandforms = allowedLandforms;
        MinMoisture = res.MinMoisture;
        Color = res.Color;
        Name = name;
        MovementCostMult = res.MovementCostMult;
    }
    
    public virtual bool Allowed(MapPolygon p, float moisture, Landform lf, Data data)
    {
        // if (p.IsWater()) return false;
        return AllowedLandforms.Contains(lf) && moisture >= MinMoisture;
    }
}