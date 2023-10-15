using Godot;
using System;
using System.Collections.Generic;
using System.Reflection;

public interface IEntityMeta
{
    IRefColMeta<TProperty> GetRefColMeta<TProperty>(string fieldName);
    public static IEntityMeta ConstructFromType(Type type)
    {
        return (IEntityMeta)typeof(EntityMeta<>)
            .MakeGenericType(type)
            .GetMethod(nameof(EntityMeta<Entity>.Construct), BindingFlags.Static | BindingFlags.Public)
            .Invoke(null, new object?[]{});
    }
}
