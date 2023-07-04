using System;
using System.Collections.Generic;
using System.Linq;
using MessagePack;

public class ModelWallet : ModelWallet<IModel>
{
    protected ModelWallet(Dictionary<int, int> contents) : base(contents)
    {
    }
}
public class ModelWallet<T> : Wallet<int> where T : IModel
{
    public int this[T model] => this[model.Id];
    public static ModelWallet<T> Construct()
    {
        return new ModelWallet<T>(new Dictionary<int, int>());
    }
    public static ModelWallet<T> Construct(ModelWallet<T> toCopy)
    {
        return new ModelWallet<T>(new Dictionary<int, int>(toCopy.Contents));
    }
    [SerializationConstructor] protected ModelWallet(Dictionary<int, int> contents) : base(contents)
    {
    }

    public void Add(T model, int amount)
    {
        Add(model.Id, amount);
    }
    public void Remove(T model, int amount)
    {
        Remove(model.Id, amount);
    }
}
