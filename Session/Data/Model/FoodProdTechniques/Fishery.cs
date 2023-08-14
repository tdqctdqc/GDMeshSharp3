using System;
using System.Collections.Generic;
using System.Linq;
using Godot;

public class Fishery : FoodProdTechnique
{
    public Fishery(PeepJobList jobs) 
        : base(nameof(Fishery), 1000, 200, 10, jobs.Fisher)
    {
    }

    public override int NumForPoly(MapPolygon poly, Data data)
    {
        var val = 0f;
        var waterNs = poly.Neighbors.Items(data).Where(n => n.IsWater());
        if(waterNs.Count() > 0)
        {
            val += waterNs.Sum(n => n.Tris.Tris.Sum(t => t.GetArea()));
        }

        var riverTris = poly.Tris.Tris.Where(t => t.Landform(data) == data.Models.Landforms.River);
        if(riverTris.Count() > 0)
        {
            val +=  riverTris.Sum(t => t.GetArea()) * 50f;
        }

        if (val < 0f)
        {
            return 0;
        }
        return Mathf.CeilToInt(val / 80_000f);
    }
}
