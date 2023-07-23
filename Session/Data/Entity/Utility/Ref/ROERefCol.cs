using System;
using System.Collections.Generic;
using System.Linq;
using MessagePack;

public class ROERefCol<TRef> 
    : IReadOnlyRefCollection<TRef> where TRef : Entity
{
    public string Name { get; protected set; }
    public int OwnerEntityId { get; protected set; }
    public HashSet<int> RefIds { get; protected set; }
    protected List<TRef> _refs;
    public int Count() => RefIds.Count;
    public static ROERefCol<TRef> Construct(string name, 
        int ownerId,
        HashSet<int> refIds, Data data)
    {
        var col = new ROERefCol<TRef>(name, ownerId, refIds);
        col._refs = col.RefIds.Select(id => (TRef) data[id]).ToList();
        return col;
    }
    [SerializationConstructor] protected ROERefCol(string name, int ownerEntityId, HashSet<int> refIds)
    {
        OwnerEntityId = ownerEntityId;
        Name = name;
        RefIds = refIds == null ? new HashSet<int>() : new HashSet<int>(refIds);
        _refs = null;
    }
    public IReadOnlyList<TRef> Items(Data data)
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
    public void UpdateOwnerId(int newId, StrongWriteKey key)
    {
        OwnerEntityId = newId;
    }
}
