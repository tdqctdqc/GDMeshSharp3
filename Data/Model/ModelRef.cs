using System;
using System.Collections.Generic;
using System.Linq;
using Godot;
using MessagePack;

public struct ModelRef<T> : IdRef
    where T : class, IModel
{
    public int RefId { get; private set; }

    public ModelRef()
    {
        RefId = -1;
    }
    public ModelRef(T model, IHostWriteKey key)
    {
        RefId = model.Id;
    }

    [SerializationConstructor] public ModelRef(int refId)
    {
        RefId = refId;
    }
    IIdentifiable IdRef.Get(Data data) => Get(data);
    public T Get(Data data)
    {
        if (RefId != -1)
        {
            return data.Models.GetModel<T>(RefId);
        }

        return null;
    }

    public ModelRef<T> Copy()
    {
        return new ModelRef<T>(RefId);
    }

    public bool Fulfilled()
    {
        return RefId != -1;
    }

}