
using System.Collections.Generic;
using Godot;
using MessagePack;

public class HoldLineSubAssignment
{
    public Dictionary<int, Vector2I> BoundsByGroupId { get; private set; }

    public static HoldLineSubAssignment Construct()
    {
        return new HoldLineSubAssignment(new Dictionary<int, Vector2I>());
    }
    [SerializationConstructor] private HoldLineSubAssignment(Dictionary<int, Vector2I> boundsByGroupId) 
    {
        BoundsByGroupId = boundsByGroupId;
    }
}