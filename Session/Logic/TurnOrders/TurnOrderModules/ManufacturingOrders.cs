using System.Collections.Generic;
using MessagePack;

public class ManufacturingOrders : TurnOrderModule
{
    public List<PolymorphMessage<ManufactureProject>> ToStart { get; private set; }
    public List<int> ToCancel { get; private set; }

    public static ManufacturingOrders Construct()
    {
        return new ManufacturingOrders(new List<PolymorphMessage<ManufactureProject>>(), new List<int>());
    }
    [SerializationConstructor] private ManufacturingOrders(List<PolymorphMessage<ManufactureProject>> toStart, List<int> toCancel)
    {
        ToStart = toStart;
        ToCancel = toCancel;
    }
}