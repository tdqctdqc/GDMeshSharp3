using Godot;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using MessagePack;

public class EntityRefCollection<TRef> : IRefCollection<TRef> where TRef : Entity
{
    public string Name { get; private set; }
    public int OwnerEntityId { get; private set; }
    public HashSet<int> RefIds { get; private set; }
    private List<TRef> _refs;
    public int Count() => RefIds.Count;
    public static EntityRefCollection<TRef> Construct(string name, 
        int ownerId,
        HashSet<int> refIds, Data data)
    {
        var col = new EntityRefCollection<TRef>(name, ownerId, refIds);
        col._refs = col.RefIds.Select(id => (TRef) data[id]).ToList();
        return col;
    }
    [SerializationConstructor] private EntityRefCollection(string name, int ownerEntityId, HashSet<int> refIds)
    {
        OwnerEntityId = ownerEntityId;
        Name = name;
        RefIds = refIds == null ? new HashSet<int>() : new HashSet<int>(refIds);
        _refs = null;
    }

    public IReadOnlyList<TRef> Entities(Data data)
    {
        if (_refs == null)
        {
            data.RefFulfiller.Fulfill(this);
        }
        return _refs;
    }

    public bool Contains(TRef entity)
    {
        return RefIds.Contains(entity.Id);
    }
    public void SyncRef(Data data)
    {
        _refs = new List<TRef>();
        foreach (var id in RefIds)
        {
            TRef refer = (TRef) data[id];
            _refs.Add(refer);
        }
    }

    public void ClearRef()
    {
        RefIds.Clear();
        _refs.Clear();
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
        key.Data.GetEntityMeta(owner.GetType()).GetRefColMeta<TRef>(Name).RaiseAdded(owner, t);
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

    public void UpdateOwnerId(int newId, StrongWriteKey key)
    {
        OwnerEntityId = newId;
    }
}
