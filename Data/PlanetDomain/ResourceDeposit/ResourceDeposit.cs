
using System;
using MessagePack;

public class ResourceDeposit : Entity
{
    
    public ModelRef<Item> Item { get; protected set; }
    public CellRef Cell { get; protected set; }
    public ModelRef<ResourceExtractionBuilding> Extraction { get; private set; }
    
    public static ResourceDeposit Create(Item resource,
        Cell cell, IHostWriteKey key)
    {
        var d = new ResourceDeposit(key.Data.IdDispenser.TakeId(),
            resource.MakeRef(), 
            new ModelRef<ResourceExtractionBuilding>(),
            cell.MakeRef());
        key.Create(d);
        return d;
    }

    [SerializationConstructor] private ResourceDeposit(int id, 
        ModelRef<Item> item, 
        ModelRef<ResourceExtractionBuilding> extraction,
        CellRef cell) : base(id)
    {
        Item = item;
        Cell = cell;
        Extraction = extraction;
    }

    public void SetExtraction(ModelRef<ResourceExtractionBuilding> extraction)
    {
        Extraction = extraction;
    }
    public override void CleanUp(StrongWriteKey key)
    {
        
    }
}
