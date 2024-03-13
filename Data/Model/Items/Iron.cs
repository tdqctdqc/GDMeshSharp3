
using System;
using System.Collections.Generic;
using System.Linq;
using Godot;

public class Iron : NaturalResource, IMineable
{
    public Iron() 
        : base(nameof(Iron), Colors.DarkRed,
            5f)
    {
    }
    protected override IFunction<float, float> DepositChanceFunction { get; }  = new ArctanFunction(100f);
    public override int GetDepositScore(Cell p, Data d)
    {
        var score = 15;
        score = Mathf.FloorToInt(score + p.Landform.Get(d).MinRoughness * 50);
        if (p is RiverCell || p is SeaCell) score /= 10;
        if(p is LandCell && p.Vegetation.Get(d).MinMoisture >= d.Models.Vegetations.Swamp.MinMoisture * .75f) score += 20;
        return score;
    }
}
