
using System.Collections.Generic;
using Godot;

public class Oil : NaturalResource
{
    public Oil() 
        : base(nameof(Oil), Colors.Purple, 
            5)
    {
    }

    public override float GetDepositChance(Cell p, Data d)
    {
        if (p is not LandCell) return .01f;
        var score = 0f;
        score = (1f - p.Landform.Get(d).MinRoughness) / 100f;
        return score;
    }

}
