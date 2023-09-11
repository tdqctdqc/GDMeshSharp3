using MessagePack;

public class WaypointPolymorph : Polymorph
{
    public Waypoint Waypoint() => (Waypoint)Value;

    public static WaypointPolymorph Construct(Waypoint value)
    {
        return new WaypointPolymorph(value);
    }
    [SerializationConstructor] public WaypointPolymorph(object value) : base(value)
    {
    }
}