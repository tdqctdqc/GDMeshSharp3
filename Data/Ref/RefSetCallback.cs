
using System;
using System.Collections.Generic;
using System.Linq;
using MessagePack;

public class RefSetCallback<TRef> : RefSet<TRef>
    where TRef : IdRef
{
    protected Action<TRef, Data> _add, _remove;
    
    
    [SerializationConstructor] protected RefSetCallback(HashSet<TRef> items) 
        : base(items)
    {
    }

    public void SetCallbacks(Action<TRef, Data> add, Action<TRef, Data> remove)
    {
        _add = add;
        _remove = remove;
    }
    
    public void Add(List<TRef> ids, StrongWriteKey key)
    {
        ids.ForEach(id => Add(id, key));
    }
    public void Add(TRef t, StrongWriteKey key)
    {
        Add(t, key);
        _add(t, key.Data);
    }
    public void Remove(List<TRef> ids, StrongWriteKey key)
    {
        ids.ForEach(id => Remove(id, key));
    }
    public void Remove(TRef t, StrongWriteKey key)
    {
        Remove(t, key);
        _remove(t, key.Data);
    }
}


