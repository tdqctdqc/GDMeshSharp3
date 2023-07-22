
using System;

public class BuildingAux : EntityAux<MapBuilding>
{
    public AuxMultiIndexer<MapPolygon, MapBuilding> ByPoly { get; private set; }
    public PropEntityIndexer<MapBuilding, PolyTriPosition> ByTri { get; private set; }
    public BuildingAux(Data data) : base(data)
    {
        ByPoly = AuxMultiIndexer<MapPolygon, MapBuilding>.ConstructConstant(
            data, 
            b => b.Position.Poly(data));
        ByTri = PropEntityIndexer<MapBuilding, PolyTriPosition>.CreateConstant(
            data, 
            b => b.Position);
    }
}
