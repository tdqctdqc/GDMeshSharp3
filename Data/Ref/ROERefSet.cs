using System;
using System.Collections.Generic;
using System.Linq;
using MessagePack;

public class ROERefSet<TRef> 
    : IReadOnlyRefCollection<TRef> where TRef : Entity
{
    public string Name { get; protected set; }
    public int OwnerEntityId { get; protected set; }
    public HashSet<int> RefIds { get; protected set; }
    public int Count() => RefIds.Count;
    public static ROERefSet<TRef> Construct(string name, 
        int ownerId,
        HashSet<int> refIds, Data data)
    {
        var col = new ROERefSet<TRef>(name, ownerId, refIds);
        return col;
    }
    [SerializationConstructor] protected ROERefSet(string name, int ownerEntityId, HashSet<int> refIds)
    {
        OwnerEntityId = ownerEntityId;
        Name = name;
        RefIds = refIds == null ? new HashSet<int>() : new HashSet<int>(refIds);
    }
    public IEnumerable<TRef> Items(Data data)
    {
        return RefIds
            .Select(id => (TRef) data[id]);
    }
    public bool Contains(TRef entity)
    {
        if (entity == null) return false;
        if (RefIds == null) return false;
        return RefIds.Contains(entity.Id);
    }
    public void UpdateOwnerId(int newId, StrongWriteKey key)
    {
        OwnerEntityId = newId;
    }
}
