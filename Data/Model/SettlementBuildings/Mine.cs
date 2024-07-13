using System;
using System.Collections.Generic;
using System.Linq;
using Godot;

public class Mine : ResourceExtractionBuilding
{
    public Mine() 
    {
    }
    public override bool CanBuildInCell(Cell t, Data data)
    {
        return t is LandCell
            && data.Planet.ResourceDepositAux.ByCell[t].Item
                .Get(data) == Resource(data);
    }
}
