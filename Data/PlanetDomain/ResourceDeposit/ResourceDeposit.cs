
using System;
using MessagePack;

public class ResourceDeposit : Entity
{
    public ModelRef<Item> Item { get; protected set; }
    public CellRef Cell { get; protected set; }
    public static ResourceDeposit Create(Item resource,
        Cell cell, ICreateWriteKey key)
    {
        var d = new ResourceDeposit(key.Data.IdDispenser.TakeId(),
            resource.MakeRef(), cell.MakeRef());
        key.Create(d);
        return d;
    }

    [SerializationConstructor] private ResourceDeposit(int id, 
        ModelRef<Item> item, CellRef cell) : base(id)
    {
        Item = item;
        Cell = cell;
    }


    public override void CleanUp(StrongWriteKey key)
    {
        
    }
}
