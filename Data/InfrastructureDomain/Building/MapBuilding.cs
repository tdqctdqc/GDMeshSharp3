using System;
using MessagePack;

public class MapBuilding : Entity
{
    public CellRef Cell { get; protected set; }
    public ModelRef<BuildingModel> Model { get; protected set; }
    public static MapBuilding Create(Cell cell, 
        BuildingModel model, ICreateWriteKey key)
    {
        var b = new MapBuilding(key.Data.IdDispenser.TakeId(), 
            cell.MakeRef(), model.MakeRef());
        key.Create(b);
        return b;
    }
    public static MapBuilding CreateGen(Cell cell, 
        BuildingModel model, GenWriteKey key)
    {
        var b = new MapBuilding(key.Data.IdDispenser.TakeId(), 
            cell.MakeRef(), model.MakeRef());
        key.Create(b);
        return b;
    }
    [SerializationConstructor] private MapBuilding(int id, 
        CellRef cell, 
        ModelRef<BuildingModel> model) : base(id)
    {
        Cell = cell;
        Model = model;
    }

    public override void CleanUp(StrongWriteKey key)
    {
        
    }
}
