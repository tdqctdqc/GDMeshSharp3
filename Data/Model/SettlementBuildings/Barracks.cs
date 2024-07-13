using System.Collections.Generic;
using Godot;

public class Barracks : SettlementBuilding
{
    public Barracks() 
    {
    }

    public override bool CanBuildInCell(Cell t, Data data)
    {
        return t is LandCell;
    }

    public override bool CanBuildInPoly(MapPolygon p, Data data)
    {
        return p.IsLand;
    }
}