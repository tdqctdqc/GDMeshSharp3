
using System;
using Godot;

public class InfantryMoveType : MoveType
{
    public InfantryMoveType() 
        : base(true, 100f, 
            nameof(InfantryMoveType))
    {
        
    }

    public override float TerrainCostInstantaneous(PolyTri pt, Data d)
    {
        var lf = pt.Landform(d);
        if (lf.IsWater)
        {
            if (lf is River) return 5f;
            return Mathf.Inf;
        }
        var lfMod = 1f + lf.MinRoughness;
        var vMod = pt.Vegetation(d).MovementCostMult;
        return lfMod * vMod;
    }

    public override bool TerrainPassable(PolyTri pt, Data d)
    {
        return pt.Landform(d).IsLand;
    }
}