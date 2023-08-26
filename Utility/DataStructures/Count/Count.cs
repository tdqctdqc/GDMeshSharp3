
using System;
using System.Collections.Generic;
using System.Linq;
using Godot;
using MessagePack;

public abstract class Count<T>
{
    public Dictionary<T, float> Contents { get; private set; }
    public bool CanBeNegative { get; private set; }
    public float this[T t] => Contents.ContainsKey(t) ? Contents[t] : 0;
    [SerializationConstructor] protected Count(Dictionary<T, float> contents, bool canBeNegative)
    {
        Contents = contents;
        CanBeNegative = canBeNegative;
    }
    public void Add(T t, float amount)
    {
        if (amount < 0f) throw new Exception("Trying to add negative amount to wallet");
        if(Contents.ContainsKey(t) == false) Contents.Add(t, 0);
        Contents[t] += amount;
    }
    public void Remove(T t, float amount)
    {
        if (amount < 0f) throw new Exception("Trying to remove negative amount from wallet");
        if(Contents.ContainsKey(t) == false)
        {
            throw new Exception("Trying to remove whats not in wallet");
        }
        if(Contents[t] < amount && CanBeNegative == false)
        {
            throw new Exception("Trying to remove more than in wallet");
        }
        Contents[t] -= amount;
    }
    
    public void Clear()
    {
        Contents.Clear();
    }
}
