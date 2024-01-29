using GDMeshSharp3.Utility;
using MessagePack;

public class RoadNetwork : Entity
{
    public IdGraphLite<PolyCell, ModelRef<RoadModel>> Roads { get; private set; }
    public RoadModel Get(PolyCell t1, PolyCell t2, Data data) 
        => Roads[t1, t2]?.Model(data);
    
    public static RoadNetwork Create(GenWriteKey key)
    {
        var n = new RoadNetwork(key.Data.IdDispenser.TakeId(), IdGraphLite<PolyCell, ModelRef<RoadModel>>.Construct());
        key.Create(n);
        return n;
    }
    [SerializationConstructor] private RoadNetwork(int id, IdGraphLite<PolyCell, ModelRef<RoadModel>> roads) : base(id)
    {
        Roads = roads;
    }

    public override void CleanUp(StrongWriteKey key)
    {
        
    }
}