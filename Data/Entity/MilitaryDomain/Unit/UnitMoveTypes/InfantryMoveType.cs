
public class InfantryMoveType : UnitMoveType
{
    public InfantryMoveType() 
        : base(true, 1f, nameof(InfantryMoveType))
    {
        
    }

    public override float TerrainSpeedMod(PolyTri pt, Data d)
    {
        if (pt.Landform(d).IsWater) return 0f;
        var lf = 1f - pt.Landform(d).MinRoughness / 2f;
        var v = pt.Vegetation(d).MovementMod;
        return lf * v;
    }

    public override bool Passable(Waypoint wp, Alliance a, Data d)
    {
        if (wp is ILandWaypoint) return false;
        return CanPassByAlliance(a, wp, d);
    }
}