using System;
using System.Collections.Generic;
using System.Linq;
using Godot;
using MessagePack;

public class PolymorphWrapper<T>
{
    public Polymorph Polymorph { get; private set; }
    public T Value() => (T)Polymorph.Value;
    
    public static PolymorphWrapper<T> Construct(T t)
    {
        return new PolymorphWrapper<T>(new DefaultPolymorph(t));
    }
    [SerializationConstructor] public PolymorphWrapper(Polymorph polymorph)
    {
        Polymorph = polymorph;
    }
}
