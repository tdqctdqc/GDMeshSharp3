
using System;
using Godot;

public class InfantryMoveType : MoveType
{
    public InfantryMoveType() 
        : base(true, 
            100f, 
            nameof(InfantryMoveType))
    {
        
    }

    public override float TerrainCostInstantaneous(PolyCell pt, Data d)
    {
        if (pt is LandCell == false) return Mathf.Inf;
        var lf = pt.GetLandform(d);
        var lfMod = 1f + lf.MinRoughness;
        var vMod = pt.GetVegetation(d).MovementCostMult;
        return lfMod * vMod;
    }

    public override bool TerrainPassable(PolyCell pt, Data d)
    {
        return pt is LandCell;
    }
}