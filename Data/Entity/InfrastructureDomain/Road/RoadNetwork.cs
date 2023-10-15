using GDMeshSharp3.Utility;
using MessagePack;

public class RoadNetwork : Entity
{
    public IdGraph<Waypoint, ModelRef<RoadModel>> Roads { get; private set; }
    public RoadModel Get(Waypoint t1, Waypoint t2, Data data) 
        => Roads[t1, t2]?.Model(data);
    
    public static RoadNetwork Create(GenWriteKey key)
    {
        var n = new RoadNetwork(key.Data.IdDispenser.TakeId(), IdGraph<Waypoint, ModelRef<RoadModel>>.Construct());
        key.Create(n);
        return n;
    }
    [SerializationConstructor] private RoadNetwork(int id, IdGraph<Waypoint, ModelRef<RoadModel>> roads) : base(id)
    {
        Roads = roads;
    }
}