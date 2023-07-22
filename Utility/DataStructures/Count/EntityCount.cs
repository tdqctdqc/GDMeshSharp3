using System;
using System.Collections.Generic;
using System.Linq;
using MessagePack;

public class EntityCount<T> : Count<int> 
    where T : Entity
{
    public static EntityCount<T> Construct()
    {
        return new EntityCount<T>(new Dictionary<int, float>());
    }
    [SerializationConstructor] private EntityCount(Dictionary<int, float> contents) : base(contents, false)
    {
    }
    
    public void Add(T t, int amount)
    {
        Add(t.Id, amount);
    }
    public void Remove(T t, int amount)
    {
        Remove(t.Id, amount);
    }
}
