using System;
using System.Collections.Generic;
using System.Linq;
using MessagePack;

public abstract class AddToRefColProcedure<TKey> : Procedure
{
    public EntityRef<Entity> Entity { get; private set; }
    public string CollectionName { get; private set; }
    public List<TKey> ToAdd { get; private set; }

    [SerializationConstructor] protected AddToRefColProcedure(EntityRef<Entity> entity, 
        string collectionName, List<TKey> toAdd)
    {
        Entity = entity;
        CollectionName = collectionName;
        ToAdd = toAdd;
    }

    public override bool Valid(Data data)
    {
        return Entity.CheckExists(data);
    }
    public override void Enact(ProcedureWriteKey key)
    {
        var e = Entity.Entity();
        var meta = e.GetMeta();
        var col = meta.GetRefCollection<TKey>(CollectionName, e, key);
        col.AddByProcedure(ToAdd, key);
    }
}
