using Godot;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using MessagePack;

public class ERefSet<TRef> 
    : RefSet<ERef<TRef>> where TRef : Entity
{
    public static ERefSet<TRef> Construct(IEnumerable<ERef<TRef>> items)
    {
        var col = new ERefSet<TRef>(items.ToHashSet());
        return col;
    }
    public static ERefSet<TRef> Construct(HashSet<int> items)
    {
        var col = new ERefSet<TRef>(items.Select(id => new ERef<TRef>(id)).ToHashSet());
        return col;
    }
    [SerializationConstructor] private ERefSet(
        HashSet<ERef<TRef>> refs) 
        : base(refs)
    {
    }

    public IEnumerable<TRef> Entities(Data d)
    {
        return Refs.Select(r => r.Get(d));
    }
    public void Add(List<TRef> ids, IWriteKey key)
    {
        ids.ForEach(id => Add(id, key));
    }
    public void Add(TRef t, IWriteKey key)
    {
        Add(t.MakeRef(), key);
    }
    public void Remove(List<TRef> ids, IWriteKey key)
    {
        ids.ForEach(id => Remove(id, key));
    }
    public void Remove(TRef t, IWriteKey key)
    {
        Remove(t.MakeRef(), key);
    }
    public bool Contains(int id)
    {
        return Contains(new ERef<TRef>(id));
    }
    public bool Contains(TRef t)
    {
        return Contains(t.MakeRef());
    }

    public void Clear(IWriteKey key)
    {
        base.Clear(key);
    }
}
