using Godot;
using System;
using System.Collections.Generic;
using MessagePack;

public sealed class EntityVarUpdate : Update
{
    public string FieldName { get; private set; }
    public int EntityId { get; private set; }
    public byte[] NewValBytes { get; private set; }
    public static EntityVarUpdate Create(string fieldName, int entityId, byte[] newVal, HostWriteKey key)
    {
        return new EntityVarUpdate(fieldName, entityId,
            newVal);
    }
    [SerializationConstructor] private EntityVarUpdate(string fieldName, int entityId, byte[] newValBytes)
    {
        FieldName = fieldName;
        EntityId = entityId;
        NewValBytes = newValBytes;
    }

    public override void Enact(ProcedureWriteKey key)
    {
        var entity = key.Data[EntityId];
        var meta = entity.GetMeta();
        var newVal = Game.I.Serializer.MP.Deserialize(NewValBytes, meta.FieldTypes[FieldName]);
        meta.UpdateEntityVar(FieldName, entity, key, newVal);
    }
}
