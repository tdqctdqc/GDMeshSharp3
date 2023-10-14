using System;
using System.Collections.Generic;
using System.Linq;
using Godot;
using MessagePack;

public class ModelRef<T> : IRef where T : class, IModel
{
    public int ModelId { get; private set; }
    public ModelRef(T model, ICreateWriteKey key)
    {
        ModelId = model.Id;
    }

    public ModelRef(int modelId)
    {
        ModelId = modelId;
    }

    public T Model(Data data)
    {
        if (ModelId != -1)
        {
            return data.Models.GetModel<T>(ModelId);
        }

        return null;
    }

    public ModelRef<T> Copy()
    {
        return new ModelRef<T>(ModelId);
    }

    public void ClearRef()
    {
        ModelId = -1;
    }
}