using System;
using System.Collections.Generic;
using System.Linq;
using Godot;

public class HeavyMetal : NaturalResource, IMineable
{
    public HeavyMetal() 
        : base(nameof(HeavyMetal), new Color("#0047AB"),
            25f)
    {
    }
    protected override IFunction<float, float> DepositChanceFunction { get; }  = new ArctanFunction(100f);
    public override int GetDepositScore(Cell p, Data d)
    {
        var score = 3;
        score = Mathf.FloorToInt(score + p.Landform.Get(d).MinRoughness * 10);
        if (p is SeaCell || p is RiverCell) score /= 10;
        return score;
    }
}
