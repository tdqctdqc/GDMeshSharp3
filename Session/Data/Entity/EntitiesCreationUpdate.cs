using System;
using System.Collections.Generic;
using System.Linq;
using MessagePack;

public class EntitiesCreationUpdate : Update
{
    public Type[] EntityTypes { get; private set; }
    public byte[][] EntityBytes { get; private set; }
    
    public static EntitiesCreationUpdate Create(IReadOnlyList<Entity> entities, CreateWriteKey key)
    {
        var entityBytes = entities.Select(e => Game.I.Serializer.MP.Serialize(e, e.GetType())).ToArray();
        var entityTypes = entities.Select(e => e.GetType()).ToArray();
        return new EntitiesCreationUpdate(entityTypes, entityBytes);
    }
    public static EntitiesCreationUpdate Create(IEnumerable<EntityCreationUpdate> updates, WriteKey key)
    {
        var entityBytes = updates.Select(e => e.EntityBytes).ToArray();
        var entityTypes = updates.Select(e => e.EntityType).ToArray();
        return new EntitiesCreationUpdate(entityTypes, entityBytes);
    }
    [SerializationConstructor] public EntitiesCreationUpdate(Type[] entityTypes, byte[][] entityBytes) 
    {
        EntityBytes = entityBytes;
        EntityTypes = entityTypes;
    }
    public override void Enact(ProcedureWriteKey key)
    {
        var es = new Entity[EntityBytes.Count()];
        for (var i = 0; i < EntityBytes.Length; i++)
        {
            var e = (Entity)Game.I.Serializer.MP.Deserialize(EntityBytes[i], EntityTypes[i]);
            es[i] = e;
        }
        key.Data.AddEntities(es, key);
    }
}
