using System;
using System.Collections.Generic;
using System.Linq;
using Godot;

public class Ranch : FoodProdTechnique
{
    public Ranch(PeepJobList list) 
        : base(nameof(Ranch), 1000, 100, 2, list.Herder)
    {
    }

    public override int NumForPoly(MapPolygon poly, Data data)
    {
        return Mathf.FloorToInt(poly.Tris.Tris
                    .Where(t =>
                        t.Landform(data).IsLand
                        && t.Landform(data).MinRoughness <= data.Models.Landforms.Hill.MinRoughness
                        && t.Vegetation(data).MinMoisture <= data.Models.Vegetations.Grassland.MinMoisture
                        && float.IsNaN(t.GetArea()) == false
                    )
                    .Sum(t =>
                    {
                        var lfMod = ShapingFunctions.ProjectToRange(t.Landform(data).FertilityMod, 1f, .25f, 1f);
                        var vMod = ShapingFunctions.ProjectToRange(t.Vegetation(data).FertilityMod, 1f, .5f, 1f);
                        var grassMod = t.Vegetation(data) == data.Models.Vegetations.Grassland ? .5f : 1f;
                        return t.GetArea() * lfMod * vMod * grassMod;
                    })
                / 5000f);
    }
}
