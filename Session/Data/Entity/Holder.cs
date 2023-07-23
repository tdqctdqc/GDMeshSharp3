using System;
using System.Collections.Generic;
using System.Linq;
using MessagePack;

public class Holder<T> : Entity
{
    public T Value { get; private set; }

    public static Holder<T> Create(T value, CreateWriteKey key)
    {
        var h = new Holder<T>(value, -1);
        key.Create(h);
        return h;
    }
    [SerializationConstructor] private Holder(T value, int id) : base(id)
    {
        Value = value;
    }
}
