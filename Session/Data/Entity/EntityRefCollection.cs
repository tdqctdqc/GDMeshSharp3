using Godot;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using MessagePack;

public class EntityRefCollection<TRef> : IRefCollection<TRef> where TRef : Entity
{
    public string Name { get; private set; }
    public HashSet<int> RefIds { get; private set; }
    private List<TRef> _refs;
    public int Count() => RefIds.Count;
    public static EntityRefCollection<TRef> Construct(string name, HashSet<int> refIds, Data data)
    {
        var col = new EntityRefCollection<TRef>(name, refIds);
        col._refs = col.RefIds.Select(id => (TRef) data[id]).ToList();
        return col;
    }
    [SerializationConstructor] private EntityRefCollection(string name, HashSet<int> refIds)
    {
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

    public void Add(Entity e, List<TRef> ids, StrongWriteKey key)
    {
        ids.ForEach(id => Add(e, id, key));
    }

    public void Add(Entity e, TRef t, StrongWriteKey key)
    {
        if (RefIds.Contains(t.Id)) return;
        RefIds.Add(t.Id);
        _refs?.Add(t);
        e.GetMeta().GetRefColMeta<TRef>(Name).RaiseAdded(e, t);
    }

    public void Remove(Entity e, List<TRef> ids, StrongWriteKey key)
    {
        ids.ForEach(id => Remove(e, id, key));
    }

    public void Remove(Entity e, TRef t, StrongWriteKey key)
    {
        RefIds.Remove(t.Id);
        _refs?.Remove(t);
        e.GetMeta().GetRefColMeta<TRef>(Name).RaiseRemoved(e, t);
    }
}
