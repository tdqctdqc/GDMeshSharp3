using System;
using System.Collections.Generic;
using System.Linq;
using MessagePack;

public class ModelCount<T> : Count<int> where T : IModel
{
    public float this[T model] => Get(model.Id);
    [SerializationConstructor] protected ModelCount(Dictionary<int, float> contents, bool canBeNegative) 
        : base(contents, canBeNegative)
    {
    }

    public void Add(T model, float amount)
    {
        Add(model.Id, amount);
    }
    public void Remove(T model, float amount)
    {
        Remove(model.Id, amount);
    }
}
