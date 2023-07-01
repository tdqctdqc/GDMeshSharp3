using System;
using System.Collections.Generic;
using System.Linq;
using Godot;
using MessagePack;

public class ModelRef<T> : IRef where T : class, IModel
{
    public int ModelId { get; private set; }
    private T _ref;

    public ModelRef(T model, CreateWriteKey key)
    {
        ModelId = model.Id;
    }

    public ModelRef(int modelId)
    {
        ModelId = modelId;
    }

    public T Model()
    {
        if (_ref == null)
        {
            Game.I.RefFulfiller.Fulfill(this);
        }

        return _ref;
    }

    public ModelRef<T> Copy()
    {
        return new ModelRef<T>(ModelId);
    }
    public void SyncRef(Data data)
    {
        _ref = data.Models.GetModel<T>(ModelId);
    }

    public void ClearRef()
    {
        _ref = null;
        ModelId = -1;
    }
}