
using System;

public class InfantryMoveType : MoveType
{
    public InfantryMoveType() 
        : base(true, 1f, nameof(InfantryMoveType))
    {
        
    }

    public override float TerrainSpeedMod(PolyTri pt, Data d)
    {
        var lf = pt.Landform(d);
        if (lf.IsWater)
        {
            if (lf is River) return .1f;
            return 0f;
        }
        var lfMod = 1f - lf.MinRoughness / 2f;
        var vMod = pt.Vegetation(d).MovementMod;
        return lfMod * vMod;
    }

    public override bool Passable(Waypoint wp, Alliance a, Data d)
    {
        if (wp is ILandWaypoint == false) return false;
        return CanPassByAlliance(a, wp, d);
    }

    public override float PathfindCost(Waypoint wp, Alliance a, Data d)
    {
        if (wp is ILandWaypoint l == false || CanPassByAlliance(a, wp, d) == false)
        {
            throw new Exception();
        }
        return l.Roughness;
    }
}