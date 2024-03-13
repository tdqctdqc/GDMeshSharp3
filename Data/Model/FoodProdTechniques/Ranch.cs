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

    public override float NumForCell(Cell t, Data data)
    {
        if (t.GetLandform(data).IsLand
            && t.GetLandform(data).MinRoughness <= data.Models.Landforms.Hill.MinRoughness
            && t.GetVegetation(data).MinMoisture <= data.Models.Vegetations.Grassland.MinMoisture
            && float.IsNaN(t.Area()) == false)
        {
            var lfMod = ShapingFunctions.ProjectToRange(t.GetLandform(data).FertilityMod, 1f, .25f, 1f);
            var vMod = ShapingFunctions.ProjectToRange(t.GetVegetation(data).FertilityMod, 1f, .5f, 1f);
            var grassMod = t.GetVegetation(data) == data.Models.Vegetations.Grassland ? .5f : 1f;
            return t.Area() * lfMod * vMod * grassMod / 5000f;
        }

        return 0f;
    }
}
