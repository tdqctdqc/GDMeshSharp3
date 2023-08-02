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

    public override int NumForPoly(MapPolygon poly, Data data)
    {
        return Mathf.FloorToInt(poly.Tris.Tris
                    .Where(t =>
                        t.Landform(data).IsLand
                        && t.Landform(data).MinRoughness <= data.Models.Landforms.Hill.MinRoughness
                        && t.Vegetation(data).MinMoisture < data.Models.Vegetations.Grassland.MinMoisture
                        && float.IsNaN(t.GetArea()) == false
                    )
                    .Sum(t => t.GetArea()
                              * t.Landform(data).FertilityMod
                              * ShapingFunctions.ProjectToRange(t.Vegetation(data).FertilityMod, 1f, .5f, 1f))
                / 10000f);
    }
}
