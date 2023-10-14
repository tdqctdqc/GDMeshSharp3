using Godot;
using System;
using MessagePack;

public class EntityDeletionUpdate : Update
{
    public int EntityId { get; private set; }
    public static EntityDeletionUpdate Create(int entityId, HostWriteKey key)
    {
        return new EntityDeletionUpdate(entityId);
    }
    [SerializationConstructor] private EntityDeletionUpdate(int entityId)
    {
        EntityId = entityId;
    }
    public override void Enact(ProcedureWriteKey key)
    {
        key.Data.RemoveEntity(EntityId, key);
    }
}
