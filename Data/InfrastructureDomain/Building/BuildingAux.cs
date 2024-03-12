
using System;

public class BuildingAux
{
    public PropEntityIndexer<MapBuilding, Cell> ByCell { get; private set; }
    public BuildingAux(Data data)
    {
        ByCell = PropEntityIndexer<MapBuilding, Cell>.CreateConstant(
            data, 
            b => data.Planet.PolygonAux.PolyCells.Cells[b.Cell.RefId]);
    }
}
