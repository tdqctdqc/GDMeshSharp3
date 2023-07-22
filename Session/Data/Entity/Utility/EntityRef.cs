using Godot;
using System;
using System.Collections.Generic;
using MessagePack;

public interface IEntityRef : IRef
{
    int RefId { get; }
}
public class EntityRef<TRef> : IEntityRef where TRef : Entity
{
    public int RefId { get; private set; }
    
    public EntityRef(TRef entity)
    {
        RefId = entity.Id;
    }
    
    [SerializationConstructor] public EntityRef(int refId)
    {
        RefId = refId;
    }
    public TRef Entity(Data data)
    {
        if (RefId == -1) return null;
        return data.RefFulfiller.Get<TRef>(this);
    }

    public bool Empty()
    {
        return RefId == -1;
    }

    public bool Fulfilled()
    {
        return RefId != -1;
    }

    public void SyncRef(Data data)
    {
        if (data.Entities.ContainsKey(RefId) == false)
        {
            RefId = -1;
        }
    }

    public void ClearRef()
    {
        RefId = -1;
    }
    
    public override string ToString()
    {
        return Empty() ? "Empty" : RefId.ToString();
    }
}
