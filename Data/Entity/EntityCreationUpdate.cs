using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using MessagePack;


public sealed partial class EntityCreationUpdate : Update
{
    public Type EntityType { get; private set; }
    public byte[] EntityBytes { get; private set; }
    
    public static EntityCreationUpdate Create(Entity entity,
        Key key)
    {
        var entityBytes = key.Data.Serializer.MP.Serialize(entity, entity.GetType());
        return new EntityCreationUpdate(entity.GetType(), entityBytes);
    }
    [SerializationConstructor] private EntityCreationUpdate(Type entityType, byte[] entityBytes) 
    {
        EntityBytes = entityBytes;
        EntityType = entityType;
    }
    public override void Enact(ProcedureKey key)
    {
        var e = (Entity)key.Data.Serializer.MP.Deserialize(EntityBytes, EntityType);
        key.Data.AddEntity(e, key);
    }
}



public sealed partial class EntityCreationUpdate<T> : Update
    where T : Entity
{
    public T Entity { get; private set; }
    
    public static EntityCreationUpdate<T> Create(T entity, Key key)
    {
        return new EntityCreationUpdate<T>(entity);
    }
    [SerializationConstructor] private EntityCreationUpdate(T entity)
    {
        Entity = entity;
    }
    public override void Enact(ProcedureKey key)
    {
        key.Data.AddEntity(Entity, key);
    }
}

