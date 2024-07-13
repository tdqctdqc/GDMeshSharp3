using System;
using System.Collections.Generic;
using System.Linq;

public class Factory : SettlementBuilding
{
    public Factory()
    {
        
    }

    public override bool CanBuildInCell(Cell t, Data data)
    {
        return t is LandCell && t.GetLandform(data).MinRoughness <= data.Models.Landforms.Hill.MinRoughness;
    }

    public override bool CanBuildInPoly(MapPolygon p, Data data)
    {
        return p.IsLand;
    }
}
