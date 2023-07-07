
using System;
using MessagePack;

public class MapBuilding : Entity
{
    public override Type GetDomainType() => DomainType();
    private static Type DomainType() => typeof(SocietyDomain);
    public override EntityTypeTreeNode GetEntityTypeTreeNode() => EntityTypeTreeNode;
    public static EntityTypeTreeNode EntityTypeTreeNode { get; private set; }
    public PolyTriPosition Position { get; protected set; }
    public ModelRef<BuildingModel> Model { get; protected set; }
    public float Efficiency { get; private set; } // out of 100
    
    public static MapBuilding Create(PolyTriPosition pos, BuildingModel model, CreateWriteKey key)
    {
        var b = new MapBuilding(key.IdDispenser.GetID(), pos, model.MakeRef(), 1f);
        key.Create(b);
        return b;
    }
    public static MapBuilding CreateGen(MapPolygon poly, BuildingModel model, GenWriteKey key)
    {
        var slots = poly.PolyBuildingSlots;
        if (slots.AvailableSlots.TryGetValue(model.BuildingType, out var numSlots) == false || numSlots.Count < 1)
        {
            throw new Exception();
        }
        var pos = slots.AvailableSlots[model.BuildingType].First.Value;
        slots.AvailableSlots[model.BuildingType].RemoveFirst();
        var b = new MapBuilding(key.IdDispenser.GetID(), pos, model.MakeRef(), 1f);
        key.Create(b);
        return b;
    }
    [SerializationConstructor] private MapBuilding(int id, PolyTriPosition position, 
        ModelRef<BuildingModel> model, float efficiency) : base(id)
    {
        Position = position;
        Model = model;
        Efficiency = efficiency;
    }
}
