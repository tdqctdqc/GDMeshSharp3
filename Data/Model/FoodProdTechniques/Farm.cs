using System;
using System.Collections.Generic;
using System.Linq;
using Godot;

public class Farm : FoodProdTechnique
{
    public Farm(PeepJobList list, Items items) 
        : base(nameof(Farm), 1500, 500, list.Farmer, items)
    {
    }

    public override float NumForCell(Cell t, Data data)
    {
        if (t.GetLandform(data).MinRoughness <= data.Models.Landforms.Hill.MinRoughness
            && t.GetVegetation(data).MinMoisture >= data.Models.Vegetations.Arid.MinMoisture
            )
        {
            return t.GetLandform(data).FertilityMod * t.GetVegetation(data).FertilityMod;
        }

        return 0;
    }
}
