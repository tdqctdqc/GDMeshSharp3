using System;
using MessagePack;

public class MapBuilding : Entity
{
    public int AssocWaypoint { get; private set; }
    public PolyTriPosition Position { get; protected set; }
    public ModelRef<BuildingModel> Model { get; protected set; }
    
    public static MapBuilding Create(PolyTriPosition pos, int assocWaypoint, BuildingModel model, CreateWriteKey key)
    {
        var b = new MapBuilding(-1, pos, model.MakeRef(), assocWaypoint);
        key.Create(b);
        return b;
    }
    public static MapBuilding CreateGen(MapPolygon poly, int assocWaypoint, BuildingModel model, GenWriteKey key)
    {
        var slots = poly.PolyBuildingSlots;
        if (slots.AvailableSlots.TryGetValue(model.BuildingType, out var numSlots) == false || numSlots.Count < 1)
        {
            return null;
            // throw new Exception();
        }
        var pos = slots.AvailableSlots[model.BuildingType].First.Value;
        slots.AvailableSlots[model.BuildingType].RemoveFirst();
        var b = new MapBuilding(-1, pos, model.MakeRef(), assocWaypoint);
        key.Create(b);
        return b;
    }
    [SerializationConstructor] private MapBuilding(int id, PolyTriPosition position, 
        ModelRef<BuildingModel> model, int assocWaypoint) : base(id)
    {
        Position = position;
        Model = model;
        AssocWaypoint = assocWaypoint;
    }
}
