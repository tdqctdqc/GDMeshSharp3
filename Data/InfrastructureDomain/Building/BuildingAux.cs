
using System;

public class BuildingAux
{
    public AuxMultiIndexer<MapPolygon, MapBuilding> ByPoly { get; private set; }
    public PropEntityIndexer<MapBuilding, Cell> ByCell { get; private set; }
    public RefAction<MapBuilding> BuildingCreated { get; private set; }
    public BuildingAux(Data data)
    {
        ByPoly = AuxMultiIndexer<MapPolygon, MapBuilding>.ConstructConstant(
            data, 
            b => b.Polygon.Entity(data));
        ByCell = PropEntityIndexer<MapBuilding, Cell>.CreateConstant(
            data, 
            b => data.Planet.PolygonAux.PolyCells.Cells[b.PolyCellId]);
    }
}
