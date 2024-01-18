
using System;
using Godot;

public class InfantryMoveType : MoveType
{
    public InfantryMoveType() 
        : base(true, 100f, 
            nameof(InfantryMoveType))
    {
        
    }

    public override float TerrainCostInstantaneous(PolyCell pt, Data d)
    {
        var lf = pt.GetLandform(d);
        if (lf.IsWater)
        {
            if (lf is River) return 5f;
            return Mathf.Inf;
        }
        var lfMod = 1f + lf.MinRoughness;
        var vMod = pt.GetVegetation(d).MovementCostMult;
        return lfMod * vMod;
    }

    public override bool TerrainPassable(PolyCell pt, Data d)
    {
        return pt is LandCell;
    }
}