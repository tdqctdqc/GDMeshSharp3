using System;
using MessagePack;

public class MapBuilding : Entity
{
    public int PolyCellId { get; protected set; }
    public ModelRef<BuildingModel> Model { get; protected set; }
    public EntityRef<MapPolygon> Polygon { get; private set; }
    public static MapBuilding Create(PolyCell cell, 
        MapPolygon polygon,
        BuildingModel model, ICreateWriteKey key)
    {
        var b = new MapBuilding(key.Data.IdDispenser.TakeId(), 
            cell.Id, model.MakeRef(), polygon.MakeRef());
        key.Create(b);
        return b;
    }
    public static MapBuilding CreateGen(MapPolygon poly, 
        BuildingModel model, GenWriteKey key)
    {
        var slots = poly.PolyBuildingSlots;
        if (slots.AvailableSlots.TryGetValue(model.BuildingType, out var numSlots) == false || numSlots.Count < 1)
        {
            return null;
            // throw new Exception();
        }
        var pos = slots.AvailableSlots[model.BuildingType].First.Value;
        slots.AvailableSlots[model.BuildingType].RemoveFirst();
        var b = new MapBuilding(key.Data.IdDispenser.TakeId(), 
            pos, model.MakeRef(), poly.MakeRef());
        key.Create(b);
        return b;
    }
    [SerializationConstructor] private MapBuilding(int id, int polyCellId, 
        ModelRef<BuildingModel> model, EntityRef<MapPolygon> polygon) : base(id)
    {
        PolyCellId = polyCellId;
        Model = model;
        Polygon = polygon;
    }
}
