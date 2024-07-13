using System;
using System.Collections.Generic;
using System.Linq;

public class Bank : SettlementBuilding
{
    public Bank() 
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
