using Godot;
using System;
using System.Collections.Generic;

public interface IEntityMeta
{
    Type EntityType { get; }
    Type DomainType { get; }
    IReadOnlyList<string> FieldNameList { get; }
    HashSet<string> FieldNameHash { get; }
    IReadOnlyDictionary<string, Type> FieldTypes { get; }
    IReadOnlyDictionary<string, IEntityVarMeta> Vars { get; }
    object[] GetPropertyValues(Entity entity);
    void UpdateEntityVar<TProperty>(string fieldName, Entity t, StrongWriteKey key, TProperty newValue);
    IRefCollection<TKey> GetRefCollection<TKey>(string fieldName, Entity t, ProcedureWriteKey key);
    IRefColMeta<TProperty> GetRefColMeta<TProperty>(string fieldName);
    bool TestSerialization(Entity e);
}
