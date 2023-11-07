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
        
    }

    public override bool Valid(Data data)
    {
        return true;
    }
    
    
}