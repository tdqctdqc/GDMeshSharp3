using System;
using System.Collections.Generic;
using System.Linq;
using Godot;

public class Farm : FoodProdTechnique
{
    public Farm() 
        : base(nameof(Farm), 1500, 500, 2)
    {
    }

    public override int NumForPoly(MapPolygon poly)
    {
        return Mathf.FloorToInt(poly.Tris.Tris
                    .Where(t =>
                        t.Landform.IsLand
                        && t.Landform.MinRoughness <= LandformManager.Hill.MinRoughness
                        && t.Vegetation.MinMoisture >= VegetationManager.Arid.MinMoisture
                        && float.IsNaN(t.GetArea()) == false
                    )
                    .Sum(t => t.GetArea() * t.Landform.FertilityMod * t.Vegetation.FertilityMod)
                / 5000f);
    }
}
