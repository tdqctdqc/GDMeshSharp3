
using System;
using System.Collections.Generic;
using System.Linq;
using Godot;

public class Iron : NaturalResource, IMineable
{
    protected override int _overflowSize { get; } = 100;
    protected override int _minDepositSize { get; } = 10;
    protected override OverFlowType _overflow { get; } = OverFlowType.Single;
    
    
    public Iron() 
        : base(nameof(Iron), Colors.DarkRed,
            5f)
    {
    }
    protected override IFunction<float, float> DepositChanceFunction { get; }  = new ArctanFunction(100f);
    public override int GetDepositScore(MapPolygon p, Data d)
    {
        var score = 15;
        score = Mathf.FloorToInt(score + p.Roughness * 50);
        if (p.IsWater()) score /= 10;
        if(p.IsLand && p.Moisture >= d.Models.Vegetations.Swamp.MinMoisture * .75f) score += 20;
        return score;
    }

    public override int GenerateDepositSize(MapPolygon p)
    {
        return Mathf.FloorToInt(500 * Game.I.Random.RandfRange(.5f, 2f));
    }
}
