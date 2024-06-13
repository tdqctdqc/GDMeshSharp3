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
        HashSet<ERef<TRef>> items) 
        : base(items)
    {
    }

    public IEnumerable<TRef> Entities(Data d)
    {
        return Items.Select(r => r.Get(d));
    }
    public void Add(List<TRef> ids, StrongWriteKey key)
    {
        ids.ForEach(id => Add(id, key));
    }
    public void Add(TRef t, StrongWriteKey key)
    {
        Add(t.MakeRef(), key);
    }
    public void Remove(List<TRef> ids, StrongWriteKey key)
    {
        ids.ForEach(id => Remove(id, key));
    }
    public void Remove(TRef t, StrongWriteKey key)
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
}
