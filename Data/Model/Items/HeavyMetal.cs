using System;
using System.Collections.Generic;
using System.Linq;
using Godot;

public class HeavyMetal : NaturalResource
{
    public HeavyMetal() 
        : base(nameof(HeavyMetal), new Color("#0047AB"),
            25f)
    {
    }
    public override float GetDepositChance(Cell p, Data d)
    {
        if (p is not LandCell) return 0f;
        var score = 0f;
        score += p.Landform.Get(d).MinRoughness / 50f;
        return score;
    }
}
