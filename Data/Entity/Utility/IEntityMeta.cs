using Godot;
using System;
using System.Collections.Generic;
using System.Reflection;

public interface IEntityMeta
{
    Type EntityType { get; }
    IReadOnlyList<string> FieldNameList { get; }
    IReadOnlyDictionary<string, Type> FieldTypes { get; }
    object[] GetPropertyValues(Entity entity);
    IRefColMeta<TProperty> GetRefColMeta<TProperty>(string fieldName);
    bool TestSerialization(Entity e, Data data);
    public static IEntityMeta ConstructFromType(Type type)
    {
        return (IEntityMeta)typeof(EntityMeta<>)
            .MakeGenericType(type)
            .GetMethod(nameof(EntityMeta<Entity>.Construct), BindingFlags.Static | BindingFlags.Public)
            .Invoke(null, new object?[]{});
    }
}
