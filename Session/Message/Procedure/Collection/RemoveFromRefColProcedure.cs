using System;
using System.Collections.Generic;
using System.Linq;
using MessagePack;

public abstract class RemoveFromRefColProcedure<TKey> : Procedure
{
    public EntityRef<Entity> Entity { get; private set; }
    public string CollectionName { get; private set; }
    public List<TKey> ToRemove { get; private set; }

    [SerializationConstructor] protected RemoveFromRefColProcedure(EntityRef<Entity> entity, string collectionName, 
        List<TKey> toRemove)
    {
        Entity = entity;
        CollectionName = collectionName;
        ToRemove = toRemove;
    }

    public override bool Valid(Data data)
    {
        return Entity.CheckExists(data);
    }
    public override void Enact(ProcedureWriteKey key)
    {
        var e = Entity.Entity(key.Data);
        var meta = e.GetMeta();
        var col = meta.GetRefCollection<TKey>(CollectionName, e, key);
        col.RemoveByProcedure(ToRemove, key);
    }
}
