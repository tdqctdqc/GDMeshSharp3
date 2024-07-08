using System;
using System.Collections.Generic;
using System.Linq;
using Godot;

public class Coal : NaturalResource
{
    public Coal() 
        : base(nameof(Coal), Colors.Black, 
            5)
    {
    }

    public override float GetDepositChance(Cell p, Data d)
    {
        if (p is not LandCell) return 0f;
        var score = 0f;
        score += p.Landform.Get(d).MinRoughness / 5f;
        if(p.Vegetation.Get(d).MinMoisture >= d.Models.Vegetations.Swamp.MinMoisture * .75f) 
            score *= 1.5f;
        return score;
    }
}
