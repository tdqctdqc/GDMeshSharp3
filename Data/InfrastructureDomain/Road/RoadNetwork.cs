using GDMeshSharp3.Utility;
using MessagePack;

public class RoadNetwork : Entity
{
    public IdGraphLite<Cell, ModelRef<RoadModel>> Roads { get; private set; }
    public RoadModel Get(Cell t1, Cell t2, Data data) 
        => Roads[t1, t2]?.Get(data);
    
    public static RoadNetwork Create(GenWriteKey key)
    {
        var n = new RoadNetwork(key.Data.IdDispenser.TakeId(), IdGraphLite<Cell, ModelRef<RoadModel>>.Construct());
        key.Create(n);
        return n;
    }
    [SerializationConstructor] private RoadNetwork(int id, IdGraphLite<Cell, ModelRef<RoadModel>> roads) : base(id)
    {
        Roads = roads;
    }

    public override void CleanUp(StrongWriteKey key)
    {
        
    }
}