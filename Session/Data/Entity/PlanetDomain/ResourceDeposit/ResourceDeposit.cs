
using System;
using MessagePack;

public class ResourceDeposit : Entity
{
    public ModelRef<Item> Item { get; protected set; }
    public int Size { get; protected set; }
    public EntityRef<MapPolygon> Poly { get; protected set; }
    public static ResourceDeposit Create(Item resource, MapPolygon poly, int size, CreateWriteKey key)
    {
        var d = new ResourceDeposit(-1, resource.MakeRef(), poly.MakeRef(), size);
        key.Create(d);
        return d;
    }

    [SerializationConstructor] private ResourceDeposit(int id, ModelRef<Item> item, EntityRef<MapPolygon> poly, int size) : base(id)
    {
        Item = item;
        Size = size;
        Poly = poly;
    }

    
}
