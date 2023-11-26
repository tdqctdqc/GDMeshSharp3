using System;
using System.Collections.Generic;
using System.Linq;
using Godot;
using MessagePack;

public class IdCount<T> : Count<int> 
    
    where T : IIdentifiable
{
    public float Get(T t) => Get(t.Id);
    public static IdCount<T> Construct()
    {
        return new IdCount<T>(new Dictionary<int, float>(), false);
    }
    public static IdCount<T> Construct(IdCount<T> toCopy)
    {
        return new IdCount<T>(new Dictionary<int, float>(toCopy.Contents), false);
    }
    public static IdCount<TSuper> Construct<TSuper, TSub>(IdCount<TSub> toCopy)
        where TSuper : IIdentifiable where TSub : IIdentifiable
    {
        return new IdCount<TSuper>(new Dictionary<int, float>(toCopy.Contents), false);
    }
    public static IdCount<T> Construct(Dictionary<T, float> toCopy)
    {
        return new IdCount<T>(toCopy.ToDictionary(kvp => kvp.Key.Id, kvp => kvp.Value),
            false);
    }
    [SerializationConstructor] private IdCount(Dictionary<int, float> contents, bool canBeNegative) 
        : base(contents, canBeNegative)
    {
    }
    public IEnumerable<KeyValuePair<T, float>> GetEnumerableModel(Data d)
    {
        if (typeof(IModel).IsAssignableFrom(typeof(T)) == false) throw new Exception();
        
        return Contents.Select(kvp => 
            new KeyValuePair<T, float>((T)d.Models.GetModel<IModel>(kvp.Key), 
                kvp.Value));
    }
    public IEnumerable<KeyValuePair<T, float>> GetEnumerable(Data d, Func<int, Data, T> get)
    {
        return Contents.Select(kvp => 
            new KeyValuePair<T, float>(get(kvp.Key, d), kvp.Value));
    }
    public void Add(T model, float amount)
    {
        if (amount == 0) return;
        Add(model.Id, amount);
    }
    public void Remove(T model, float amount)
    {
        if (amount == 0) return;
        try
        {
            Remove(model.Id, amount);
        }
        catch (Exception e)
        {
            GD.Print("problem removing " + model.GetType().Name);
            GD.Print("trying to remove " + amount + " only " + Get(model));
            throw;
        }
    }
    public static IdCount<T> Union<T>(params IdCount<T>[] counts)
        where T : IIdentifiable
    {
        var res = IdCount<T>.Construct();
        foreach (var count in counts)
        {
            foreach (var kvp in count.Contents)
            {
                res.Add(kvp.Key, kvp.Value);
            }
        }
        return res;
    }
    public void Subtract<T>(IdCount<T> take) where T : IIdentifiable
    {
        foreach (var kvp in take.Contents)
        {
            Remove(kvp.Key, kvp.Value);
        }
    }
}
