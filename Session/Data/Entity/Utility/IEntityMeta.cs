using Godot;
using System;
using System.Collections.Generic;

public interface IEntityMeta
{
    Type EntityType { get; }
    IReadOnlyList<string> FieldNameList { get; }
    object[] GetPropertyValues(Entity entity);
    IRefColMeta<TProperty> GetRefColMeta<TProperty>(string fieldName);
    bool TestSerialization(Entity e, Data data);
}
