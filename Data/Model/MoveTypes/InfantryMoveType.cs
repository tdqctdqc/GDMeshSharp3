
using System;
using Godot;

public class InfantryMoveType : MoveType
{
    public InfantryMoveType() 
        : base(true, 2f, 
            nameof(InfantryMoveType))
    {
        
    }

    public override float TerrainSpeedMod(PolyTri pt, Data d)
    {
        var lf = pt.Landform(d);
        if (lf.IsWater)
        {
            if (lf is River) return .2f;
            return 0f;
        }
        var lfMod = 1f - lf.MinRoughness / 2f;
        var vMod = pt.Vegetation(d).MovementMod;
        return lfMod * vMod;
    }

    public override bool Passable(Waypoint wp, Alliance a, 
        bool goThruHostile, Data d)
    {
        if (wp is ILandWaypoint == false) return false;
        return AllianceCanPass(a, wp, goThruHostile, d);
    }

    public override float PathfindCost(Waypoint wp, Alliance a, 
        bool goThruHostile, Data d)
    {
        if (wp is ILandWaypoint l == false || AllianceCanPass(a, wp, goThruHostile, d) == false)
        {
            return Mathf.Inf;
        }
        return l.Roughness;
    }
}