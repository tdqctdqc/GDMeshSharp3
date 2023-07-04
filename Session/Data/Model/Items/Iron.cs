
using System;
using System.Collections.Generic;
using System.Linq;
using Godot;

public class Iron : NaturalResource
{
    protected override int _overflowSize { get; } = 100;
    protected override int _minDepositSize { get; } = 10;
    protected override OverFlowType _overflow { get; } = OverFlowType.None;
    
    
    public Iron() 
        : base(nameof(Iron), Colors.Black.Lightened(.3f),
            5f, new MineableAttribute(), new ExtractableAttribute())
    {
    }
    protected override IFunction<float, float> DepositChanceFunction { get; }  = new ArctanFunction(100f);
    public override int GetDepositScore(MapPolygon p)
    {
        var score = 15;
        score = Mathf.FloorToInt(score + p.Roughness * 50);
        if (p.IsWater()) score /= 10;
        if(p.IsLand && p.Moisture >= VegetationManager.Swamp.MinMoisture * .75f) score += 20;
        return score;
    }

    public override int GenerateDepositSize(MapPolygon p)
    {
        return Mathf.FloorToInt(500 * Game.I.Random.RandfRange(.5f, 2f));
    }
}
