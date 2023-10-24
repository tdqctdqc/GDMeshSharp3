using System.Collections.Generic;
using MessagePack;

public class TrimFrontsProcedure : Procedure
{
    public HashSet<int> FrontsToRemove { get; private set; }
    public Dictionary<int, HashSet<int>> WaypointsToTrimByFrontId { get; private set; }

    public static TrimFrontsProcedure Construct()
    {
        return new TrimFrontsProcedure(new HashSet<int>(), new Dictionary<int, HashSet<int>>());
    }
    [SerializationConstructor] private TrimFrontsProcedure(HashSet<int> frontsToRemove,
        Dictionary<int, HashSet<int>> waypointsToTrimByFrontId)
    {
        FrontsToRemove = frontsToRemove;
        WaypointsToTrimByFrontId = waypointsToTrimByFrontId;
    }
    public override void Enact(ProcedureWriteKey key)
    {
        foreach (var i in FrontsToRemove)
        {
            var front = key.Data.Get<Front>(i);
            var regime = front.Regime.Entity(key.Data);
            regime.Military.Fronts.Remove(front, key);
            key.Data.RemoveEntity(front.Id, key);
        }

        foreach (var kvp in WaypointsToTrimByFrontId)
        {
            var front = key.Data.Get<Front>(kvp.Key);
            var toTrim = kvp.Value;
            foreach (var i in toTrim)
            {
                front.WaypointIds.Remove(i);
            }
        }
    }

    public override bool Valid(Data data)
    {
        return true;
    }
    
    
}