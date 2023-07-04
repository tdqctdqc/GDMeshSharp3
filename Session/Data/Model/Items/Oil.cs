
using System.Collections.Generic;
using Godot;

public class Oil : NaturalResource
{
    protected override int _overflowSize { get; } = 100;
    protected override int _minDepositSize { get; } = 10;
    protected override OverFlowType _overflow { get; } = OverFlowType.Multiple;

    public Oil() 
        : base(nameof(Oil), Colors.Black, 
            5, new ExtractableAttribute())
    {
    }

    protected override IFunction<float, float> DepositChanceFunction { get; }  = new ArctanFunction(100f);
    public override int GetDepositScore(MapPolygon p)
    {
        var score = 0;
        score = Mathf.FloorToInt(score + 5 * (1f - p.Roughness));
        return score;
    }

    public override int GenerateDepositSize(MapPolygon p)
    {
        return Mathf.FloorToInt(100 * Game.I.Random.RandfRange(.5f, 2f));
    }

}
