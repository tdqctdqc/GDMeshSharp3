using Godot;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using MessagePack;

public class EntRefCol<TRef> 
    : ROERefCol<TRef>, IRefCollection<TRef> where TRef : Entity
{
    public static EntRefCol<TRef> Construct(ROERefCol<TRef> c)
    {
        return new EntRefCol<TRef>(c.Name, c.OwnerEntityId, c.RefIds.ToHashSet());
    }
    public static EntRefCol<TRef> Construct(string name, 
        int ownerId,
        HashSet<int> refIds, Data data)
    {
        var col = new EntRefCol<TRef>(name, ownerId, refIds);
        col._refs = col.RefIds.Select(id => (TRef) data[id]).ToList();
        return col;
    }
    [SerializationConstructor] private EntRefCol(string name, int ownerEntityId, HashSet<int> refIds) 
        : base(name, ownerEntityId, refIds)
    {
    }

    public void Add(List<TRef> ids, StrongWriteKey key)
    {
        ids.ForEach(id => Add(id, key));
    }
    public void Add(TRef t, StrongWriteKey key)
    {
        var owner = key.Data[OwnerEntityId];
        if (RefIds.Contains(t.Id)) return;
        RefIds.Add(t.Id);
        _refs?.Add(t);
        key.Data.GetEntityMeta(owner.GetType())
            .GetRefColMeta<TRef>(Name)
            .RaiseAdded(owner, t);
    }
    public void Remove(List<TRef> ids, StrongWriteKey key)
    {
        ids.ForEach(id => Remove(id, key));
    }
    public void Remove(TRef t, StrongWriteKey key)
    {
        var owner = key.Data[OwnerEntityId];
        RefIds.Remove(t.Id);
        _refs?.Remove(t);
        key.Data.GetEntityMeta(owner.GetType())
            .GetRefColMeta<TRef>(Name).RaiseRemoved(owner, t);
    }
}
