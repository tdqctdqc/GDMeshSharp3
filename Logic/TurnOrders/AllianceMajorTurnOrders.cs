
using System.Collections.Generic;
using MessagePack;

public class AllianceMajorTurnOrders
{
    public List<(int, List<int>)> NewFrontWaypointsByRegimeId { get; private set; }
    public static AllianceMajorTurnOrders Construct()
    {
        return new AllianceMajorTurnOrders(new List<(int, List<int>)>());
    }

    [SerializationConstructor] private AllianceMajorTurnOrders(List<(int, List<int>)> newFrontWaypointsByRegimeId)
    {
        NewFrontWaypointsByRegimeId = newFrontWaypointsByRegimeId;
    }
}