using System;
using System.Collections.Generic;
using System.Linq;
using Godot;

public class HeavyMetal : NaturalResource
{
    public HeavyMetal() 
    {
    }
    public override float GetDepositChance(Cell p, Data d)
    {
        if (p is not LandCell) return 0f;
        var score = 0f;
        score += p.Landform.Get(d).MinRoughness 
                 / 100f;
        return score;
    }
}
