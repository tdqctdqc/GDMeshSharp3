
using System;
using System.Collections.Generic;
using System.Linq;
using Godot;
using MessagePack;

public abstract class Wallet<T>
{
    public Dictionary<T, int> Contents { get; private set; }
    public int this[T t] => Contents.ContainsKey(t) ? Contents[t] : 0;
    [SerializationConstructor] protected Wallet(Dictionary<T, int> contents)
    {
        Contents = contents;
    }
    public Snapshot<T, int> GetSnapshot()
    {
        return Snapshot<T, int>.Construct(Contents);
    }
    protected void Add(T t, int amount)
    {
        if (amount < 0f) throw new Exception("Trying to add negative amount to wallet");
        if(Contents.ContainsKey(t) == false) Contents.Add(t, 0);
        Contents[t] += amount;
    }
    protected void Remove(T t, int amount)
    {
        if (amount < 0f) throw new Exception("Trying to remove negative amount from wallet");
        if(Contents.ContainsKey(t) == false)
        {
            return;
            throw new Exception("Trying to remove whats not in wallet");
        }
        if(Contents[t] < amount)
        {
            throw new Exception("Trying to remove more than in wallet");
        }
        Contents[t] -= amount;
    }

    public void TransferFrom<R>(R t, int amount, Wallet<R> destination) where R : T
    {
        Remove(t, amount);
        destination.Add(t, amount);
    }
    
    public void Clear()
    {
        Contents.Clear();
    }
}
