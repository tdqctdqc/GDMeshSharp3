using System;
using System.Collections.Generic;
using System.Linq;
using Godot;
using MessagePack;

public class ModelRef<T> : IDRef<T>
    where T : class, IModel
{
    public int RefId { get; private set; }
    public ModelRef(T model, ICreateWriteKey key)
    {
        RefId = model.Id;
    }

    public ModelRef(int refId)
    {
        RefId = refId;
    }

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

}