using System;
using System.Collections.Generic;
using System.Linq;
using Godot;

public class Ranch : FoodProdTechnique
{
    public Ranch() 
        : base(nameof(Ranch), 1000, 100, 2)
    {
    }

    public override int NumForPoly(MapPolygon poly)
    {
        return Mathf.FloorToInt(poly.Tris.Tris
                    .Where(t =>
                        t.Landform.IsLand
                        && t.Landform.MinRoughness <= LandformManager.Hill.MinRoughness
                        && t.Vegetation.MinMoisture < VegetationManager.Grassland.MinMoisture
                        && float.IsNaN(t.GetArea()) == false
                    )
                    .Sum(t => t.GetArea()
                              * t.Landform.FertilityMod
                              * ShapingFunctions.ProjectToRange(t.Vegetation.FertilityMod, 1f, .5f, 1f))
                / 10000f);
    }
}
