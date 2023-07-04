using System;
using System.Collections.Generic;
using System.Linq;
using MessagePack;

public class ModelCount : ModelCount<IModel>
{
    protected ModelCount(Dictionary<int, float> contents) : base(contents)
    {
    }
}
public class ModelCount<T> : Count<int> where T : IModel
{
    public float this[T model] => this[model.Id];
    public static ModelCount<T> Construct()
    {
        return new ModelCount<T>(new Dictionary<int, float>());
    }
    public static ModelCount<T> Construct(ModelCount<T> toCopy)
    {
        return new ModelCount<T>(new Dictionary<int, float>(toCopy.Contents));
    }
    [SerializationConstructor] protected ModelCount(Dictionary<int, float> contents) : base(contents)
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
