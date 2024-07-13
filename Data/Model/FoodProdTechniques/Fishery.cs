using System;
using System.Collections.Generic;
using System.Linq;
using Godot;

public class Fishery : FoodProdTechnique
{
    public Fishery() 
    {
    }

    public override float NumForCell(Cell cell, Data data)
    {
        var val = 0f;
        var seaNs = cell.GetNeighbors(data)
            .Where(n => n is SeaCell);
        if(seaNs.Any())
        {
            val += seaNs.Count() * 1000f;
        }
        
        var riverNs = cell.GetNeighbors(data)
            .Where(n => n is RiverCell);
        if(riverNs.Any())
        {
            val += riverNs.Count() * 5000f;
        }
        

        if (val <= 0f)
        {
            return 0;
        }
        var num = Mathf.CeilToInt(val / 80_000f);
        if (num < 0 || num > 100)
        {
            throw new Exception($"{num} fisheries" +
                                $"\n{seaNs.Count()} water ns" +
                                $"\n{(seaNs.Any() == false ? 0f :
                                    seaNs.Sum(n => n.Area()))} water score" +
                                $"\n{riverNs.Count()} river cells" +
                                $"\n{(riverNs.Any() == false ? 0f :
                                    riverNs.Sum(t => t.Area()) * 50f)} river score");
            
        }
        return num;
    }
}
