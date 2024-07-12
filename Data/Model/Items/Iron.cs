
using System;
using System.Collections.Generic;
using System.Linq;
using Godot;

public class Iron : NaturalResource
{
    public Iron() 
        : base(nameof(Iron), 5f)
    {
    }
    public override float GetDepositChance(Cell p, Data d)
    {
        var score = 0f;
        if (p is not LandCell) return 0f;
        score += p.Landform.Get(d).MinRoughness 
                 / 20f;
        if(p.Vegetation.Get(d).MinMoisture 
            >= d.Models.Vegetations.Swamp.MinMoisture * .75f) 
                score *= 1.5f;
        return score;
    }
}
