
using System.Collections.Generic;
using Godot;

public class Oil : NaturalResource
{
    public Oil() 
        : base(nameof(Oil), Colors.Purple, 
            5)
    {
    }

    protected override IFunction<float, float> DepositChanceFunction { get; }  = new ArctanFunction(100f);
    public override int GetDepositScore(Cell p, Data d)
    {
        var score = 0;
        score = Mathf.FloorToInt(score + 5 * (1f - p.Landform.Get(d).MinRoughness));
        return score;
    }

}
