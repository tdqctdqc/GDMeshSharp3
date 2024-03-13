using System;
using System.Collections.Generic;
using System.Linq;
using Godot;

public class Coal : NaturalResource, IMineable
{
    public Coal() 
        : base(nameof(Coal), Colors.Black, 
            5)
    {
    }

    protected override IFunction<float, float> DepositChanceFunction { get; }  = new ArctanFunction(100f);
    public override int GetDepositScore(Cell p, Data d)
    {
        var score = 15;
        score = Mathf.FloorToInt(score + p.Landform.Get(d).MinRoughness * 30);
        if (p is LandCell == false) score /= 5;
        if(p is LandCell && p.Vegetation.Get(d).MinMoisture >= d.Models.Vegetations.Swamp.MinMoisture * .75f) score += 40;
        return score;
    }
}
