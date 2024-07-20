
using System;
using Godot;

public class InfantryMoveType : MoveType
{

    protected override float TerrainCostInstantaneous(Cell pt, Data d)
    {
        if (pt is LandCell == false) return Mathf.Inf;
        var lf = pt.GetLandform(d);
        var lfMod = 1f + lf.MinRoughness;
        var vMod = pt.GetVegetation(d).MovementCostMult;
        return lfMod * vMod;
    }

    public override bool TerrainPassable(Cell pt, Data d)
    {
        return pt is LandCell;
    }
}