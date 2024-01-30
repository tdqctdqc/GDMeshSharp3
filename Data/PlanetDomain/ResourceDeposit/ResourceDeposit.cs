
using System;
using MessagePack;

public class ResourceDeposit : Entity
{
    public ModelRef<Item> Item { get; protected set; }
    public int Size { get; protected set; }
    public ERef<MapPolygon> Poly { get; protected set; }
    public static ResourceDeposit Create(Item resource,
        MapPolygon poly, int size, ICreateWriteKey key)
    {
        var d = new ResourceDeposit(key.Data.IdDispenser.TakeId(),
            resource.MakeRef(), poly.MakeRef(), size);
        key.Create(d);
        return d;
    }

    [SerializationConstructor] private ResourceDeposit(int id, ModelRef<Item> item, ERef<MapPolygon> poly, int size) : base(id)
    {
        Item = item;
        Size = size;
        Poly = poly;
    }


    public override void CleanUp(StrongWriteKey key)
    {
        
    }
}
