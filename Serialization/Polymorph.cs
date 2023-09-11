using MessagePack;

public class Polymorph
{
    public object Value { get; private set; }

    [SerializationConstructor] public Polymorph(object value)
    {
        Value = value;
    }
}

public class WaypointPolymorph : Polymorph
{
    public Waypoint Waypoint() => (Waypoint)Value;

    public static WaypointPolymorph Construct(object value)
    {
        return new WaypointPolymorph(value);
    }
    [SerializationConstructor] public WaypointPolymorph(object value) : base(value)
    {
    }
}