using System;
using System.Collections.Generic;
using System.Linq;
using Godot;
public class PortablePolymorph<T>
{
    public Type Type { get; private set; }
    public byte[] Bytes { get; private set; }

    public static PortablePolymorph<T> Construct(T t, Data data)
    {
        return new PortablePolymorph<T>(t.GetType(), data.Serializer.MP.Serialize(t, t.GetType()));
    }
    public PortablePolymorph(Type type, byte[] bytes)
    {
        Type = type;
        Bytes = bytes;
    }

    public T Get(Data data)
    {
        return (T)data.Serializer.MP.Deserialize(Bytes, Type);
    }
}
