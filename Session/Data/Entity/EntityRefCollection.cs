using Godot;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using MessagePack;

public class EntityRefCollection<TRef> : IRef where TRef : Entity
{
    public HashSet<int> RefIds { get; private set; }
    private List<TRef> _refs;
    public int Count() => RefIds.Count;
    public static EntityRefCollection<TRef> Construct(HashSet<int> refIds, Data data)
    {
        var col = new EntityRefCollection<TRef>(refIds);
        col._refs = col.RefIds.Select(id => (TRef) data[id]).ToList();
        return col;
    }
    [SerializationConstructor] private EntityRefCollection(HashSet<int> refIds)
    {
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
    public void AddRef(TRef t, StrongWriteKey key)
    {
        if (RefIds.Contains(t.Id)) return;
        RefIds.Add(t.Id);
        _refs?.Add(t);
    }
    public void RemoveRef(TRef t, GenWriteKey key)
    {
        RefIds.Remove(t.Id);
        _refs?.Remove(t);
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

    public void AddByProcedure(List<int> ids, ProcedureWriteKey key)
    {
        RefIds.AddRange(ids);
        _refs.AddRange(ids.Select(id => (TRef)key.Data[id]));
    }

    public void AddByProcedure(int id, ProcedureWriteKey key)
    {
        RefIds.Add(id);
        _refs.Add((TRef)key.Data[id]);
    }

    public void RemoveByProcedure(List<int> ids, ProcedureWriteKey key)
    {
        ids.ForEach(id =>
        {
            RefIds.Remove(id);
            _refs.RemoveAll(e => e.Id == id);
        });
    }

    public void RemoveByProcedure(int id, ProcedureWriteKey key)
    {
        RefIds.Remove(id);
        _refs.RemoveAll(e => e.Id == id);
    }

}
