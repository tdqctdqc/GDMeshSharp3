using System;
using System.Collections.Generic;
using System.Linq;
using MessagePack;

public class PolymorphMember<T>
{
    private T _value;
    public Type Type => _value.GetType();
    public byte[] Bytes => Game.I.Client.Key.Data.Serializer.MP.Serialize(_value);
    public T Value()
    {
        return _value;
    }

    public static PolymorphMember<T> Construct(T t)
    {
        return new PolymorphMember<T>(t);
    }

    private PolymorphMember(T t)
    {
        _value = t;
    }
    [SerializationConstructor] private PolymorphMember(Type type, byte[] bytes)
    {
        _value = (T)Game.I.Client.Key.Data.Serializer.MP.Deserialize(bytes, type);
    }
}
