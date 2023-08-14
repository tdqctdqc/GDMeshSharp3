using System;
using System.Collections.Generic;
using System.Linq;
using Godot;

public class Farm : FoodProdTechnique
{
    public Farm(PeepJobList list) 
        : base(nameof(Farm), 1500, 500, 2, list.Farmer)
    {
    }

    public override int NumForPoly(MapPolygon poly, Data data)
    {
        return Mathf.FloorToInt(poly.Tris.Tris
                    .Where(t =>
                        t.Landform(data).IsLand
                        && t.Landform(data).MinRoughness <= data.Models.Landforms.Hill.MinRoughness
                        && t.Vegetation(data).MinMoisture >= data.Models.Vegetations.Arid.MinMoisture
                        && float.IsNaN(t.GetArea()) == false
                    )
                    .Sum(t => t.GetArea() * t.Landform(data).FertilityMod * t.Vegetation(data).FertilityMod)
                / 5000f);
    }
}
