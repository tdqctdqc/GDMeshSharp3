
using System.Collections.Generic;
using Godot;

public class Oil : NaturalResource
{
    public Oil() 
    {
    }

    public override float GetDepositChance(Cell p, Data d)
    {
        if (p is not LandCell) return .01f;
        var score = 0f;
        score = (1f - p.Landform.Get(d).MinRoughness) 
                / 200f;
        return score;
    }

}
