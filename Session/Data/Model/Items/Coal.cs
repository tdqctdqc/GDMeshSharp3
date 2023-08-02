using System;
using System.Collections.Generic;
using System.Linq;
using Godot;

public class Coal : NaturalResource
{
    protected override int _overflowSize { get; } = 100;
    protected override int _minDepositSize { get; } = 10;
    protected override OverFlowType _overflow { get; } = OverFlowType.Multiple;

    public Coal() 
        : base(nameof(Coal), Colors.Black, 
            5, new ExtractableAttribute(), new MineableAttribute())
    {
    }

    protected override IFunction<float, float> DepositChanceFunction { get; }  = new ArctanFunction(100f);
    public override int GetDepositScore(MapPolygon p, Data d)
    {
        var score = 0;
        score = Mathf.FloorToInt(score + 5 * (p.Roughness));
        return score;
    }

    public override int GenerateDepositSize(MapPolygon p)
    {
        return Mathf.FloorToInt(100 * Game.I.Random.RandfRange(.5f, 2f));
    }
}
