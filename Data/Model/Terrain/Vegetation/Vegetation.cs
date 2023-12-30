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
    public float MovementMod { get; private set; }
    public override string Name { get; protected set; }
    public override Color Color { get; protected set; }
    public override int Id { get; protected set; }

    public Vegetation(HashSet<Landform> allowedLandforms, 
        float minMoisture, float fertilityMod, 
        float movementMod,
        Color color, string name)
    {
        FertilityMod = fertilityMod;
        AllowedLandforms = allowedLandforms;
        MinMoisture = minMoisture;
        Color = color;
        Name = name;
        MovementMod = movementMod;
    }
    
    public virtual bool Allowed(MapPolygon p, float moisture, Landform lf, Data data)
    {
        // if (p.IsWater()) return false;
        return AllowedLandforms.Contains(lf) && moisture >= MinMoisture;
    }
}