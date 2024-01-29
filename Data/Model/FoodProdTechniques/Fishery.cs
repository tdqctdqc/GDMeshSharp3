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
            val += waterNs.Sum(n => n.GetArea(data));
        }
        
        var riverCells = poly.GetCells(data)
            .OfType<RiverCell>();
        if(riverCells.Count() > 0)
        {
            val +=  riverCells.Sum(t => t.Area()) * 50f;
        }

        if (val < 0f)
        {
            return 0;
        }
        var num = Mathf.CeilToInt(val / 80_000f);
        if (num < 0 || num > 100)
        {
            throw new Exception($"{num} fisheries" +
                                $"\n{waterNs.Count()} water ns" +
                                $"\n{(waterNs.Count() == 0 ? 0f :
                                    waterNs.Sum(n => n.GetArea(data)))} water score" +
                                $"\n{riverCells.Count()} river cells" +
                                $"\n{(riverCells.Count() == 0 ? 0f :
                                    riverCells.Sum(t => t.Area()) * 50f)} river score");
            
        }
        return num;
    }
}
