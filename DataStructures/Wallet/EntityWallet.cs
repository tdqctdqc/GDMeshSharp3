using System;
using System.Collections.Generic;
using System.Linq;
using MessagePack;

public class EntityWallet<T> : Wallet<int> 
    where T : Entity
{
    public static EntityWallet<T> Construct()
    {
        return new EntityWallet<T>(new Dictionary<int, int>());
    }
    [SerializationConstructor] private EntityWallet(Dictionary<int, int> contents) : base(contents)
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
