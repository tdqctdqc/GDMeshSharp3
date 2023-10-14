using System;
using System.Collections.Generic;
using System.Linq;
using Godot;

public class HeavyMetal : NaturalResource, IMineable
{
    protected override int _overflowSize { get; } = 100;
    protected override int _minDepositSize { get; } = 5;
    protected override OverFlowType _overflow { get; } = OverFlowType.Single;
    
    
    public HeavyMetal() 
        : base(nameof(HeavyMetal), new Color("#0047AB"),
            25f)
    {
    }
    protected override IFunction<float, float> DepositChanceFunction { get; }  = new ArctanFunction(100f);
    public override int GetDepositScore(MapPolygon p, Data d)
    {
        var score = 3;
        score = Mathf.FloorToInt(score + p.Roughness * 10);
        if (p.IsWater()) score /= 10;
        return score;
    }

    public override int GenerateDepositSize(MapPolygon p)
    {
        return Mathf.FloorToInt(50 * Game.I.Random.RandfRange(.5f, 2f));
    }
}
