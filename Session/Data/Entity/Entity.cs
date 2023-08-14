using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using MessagePack;

public abstract class Entity : IIdentifiable
{
    public int Id { get; protected set; }
    protected Entity(int id)
    {
        Id = id;
    }

    public void SetId(int id, StrongWriteKey key)
    {
        Id = id;
        var meta = key.Data.GetEntityMeta(this.GetType());
        foreach (var refCollection in meta.GetPropertyValues(this)
                     .SelectWhereOfType<object, IReadOnlyRefCollection>())
        {
            refCollection.UpdateOwnerId(id, key);
        }
    }
}
