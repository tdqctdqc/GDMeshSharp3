
using System;

public class BuildingAux : EntityAux<MapBuilding>
{
    public AuxMultiIndexer<MapPolygon, MapBuilding> ByPoly { get; private set; }
    public Entity1to1PropIndexer<MapBuilding, PolyTriPosition> ByTri { get; private set; }
    public BuildingAux(Domain domain, Data data) : base(domain, data)
    {
        ByPoly = AuxMultiIndexer<MapPolygon, MapBuilding>.ConstructConstant(
            data, 
            b => b.Position.Poly(data));
        ByTri = Entity1to1PropIndexer<MapBuilding, PolyTriPosition>.CreateConstant(
            data, 
            b => b.Position);
    }
}
