using System.Collections.Generic;
using Godot;
using MessagePack;

public class MockNav : Entity
{
    public Dictionary<int, int> PolyCenterIds { get; private set; }
    // public Dictionary<int, PolymorphMember<Waypoint>> Waypoints { get; private set; }
    public Dictionary<Vector2, List<int>> PolyNavPaths { get; private set; }
    public static MockNav Construct(int id, 
        // Dictionary<int, PolymorphMember<Waypoint>> waypoints, 
        Dictionary<int, int> polyCenterIds, 
        Dictionary<Vector2, List<int>> polyNavPaths
        )
    {
        return new MockNav(id, 
            // waypoints, 
            polyCenterIds, 
            polyNavPaths
            );
    }
    [SerializationConstructor] private MockNav(int id, 
        // Dictionary<int, PolymorphMember<Waypoint>> waypoints,
        Dictionary<int, int> polyCenterIds, 
        Dictionary<Vector2, List<int>> polyNavPaths
        ) : base(id)
    {
        // Waypoints = waypoints;
        PolyCenterIds = polyCenterIds;
        PolyNavPaths = polyNavPaths;
    }
}