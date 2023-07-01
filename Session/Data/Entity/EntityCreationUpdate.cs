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
    
    public static EntityCreationUpdate Create(Entity entity, HostWriteKey key)
    {
        var entityBytes = Game.I.Serializer.MP.Serialize(entity, entity.GetType());
        return new EntityCreationUpdate(entity.GetType(), entityBytes);
    }
    [SerializationConstructor] private EntityCreationUpdate(Type entityType, byte[] entityBytes) 
    {
        EntityBytes = entityBytes;
        EntityType = entityType;
    }
    public override void Enact(ServerWriteKey key)
    {
        var e = (Entity)Game.I.Serializer.MP.Deserialize(EntityBytes, EntityType);
        key.Data.AddEntity(e, key);
    }
}
