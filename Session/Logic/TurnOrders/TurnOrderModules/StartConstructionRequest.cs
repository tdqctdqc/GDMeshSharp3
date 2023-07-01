using System;
using System.Collections.Generic;
using System.Linq;
using MessagePack;

public class StartConstructionRequest
{
    public int BuildingModelId { get; private set; }
    public int PolyId { get; private set; }

    public static StartConstructionRequest Construct(BuildingModel model, MapPolygon poly)
    {
        return new StartConstructionRequest(model.Id, poly.Id);
    }
    [SerializationConstructor] private StartConstructionRequest(int buildingModelId, int polyId)
    {
        BuildingModelId = buildingModelId;
        PolyId = polyId;
    }
}
