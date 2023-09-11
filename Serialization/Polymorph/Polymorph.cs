using MessagePack;
[MessagePack.Union(0, typeof(DefaultPolymorph))]
[MessagePack.Union(1, typeof(WaypointPolymorph))]
public abstract class Polymorph
{
    public object Value { get; private set; }

    [SerializationConstructor] public Polymorph(object value)
    {
        Value = value;
    }
}

