using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

public class EntityMeta<TEntity> : IEntityMeta where TEntity : Entity
{
    private Dictionary<string, IRefColMeta> _refCols;

    public void ForReference()
    {
        return;
        new EntityMeta<TEntity>();
    }

    public static EntityMeta<TEntity> Construct()
    {
        return new EntityMeta<TEntity>();
    }
    public EntityMeta()
    {
        var entityType = typeof(TEntity);
        var properties = entityType.GetProperties(BindingFlags.Instance | BindingFlags.Public);
        _refCols = new Dictionary<string, IRefColMeta>();
        
        var declaredProperties = entityType
            .GetProperties(BindingFlags.DeclaredOnly 
                                    | BindingFlags.Instance | BindingFlags.Public);
        foreach (var declaredProperty in declaredProperties)
        {
            this.InvokeGeneric(nameof(SetupRefCol), 
                new []{declaredProperty.PropertyType}, 
                new []{declaredProperty});
        }
    }
    private void SetupRefCol<TProperty>(PropertyInfo prop)
    {
        var propType = typeof(TProperty);
        if (propType.IsGenericType 
            && propType.GetGenericTypeDefinition().IsAssignableFrom(typeof(EntRefCol<>)))
        {
            var genericParam = propType.GenericTypeArguments[0];
            this.InvokeGeneric(nameof(SetupColType),
                new[] {genericParam}, new []{prop});
        }
    }
    private void SetupColType<TColMember>(PropertyInfo prop)
    {
        var col = new RefColMeta<TEntity, TColMember>();
        _refCols.Add(prop.Name, col);
    }
    IRefColMeta<TColMember> IEntityMeta.GetRefColMeta<TColMember>(string fieldName)
    {
        return GetRefColMeta<TColMember>(fieldName);
    }
    public RefColMeta<TEntity, TColMember> GetRefColMeta<TColMember>(string fieldName)
    {
        return (RefColMeta<TEntity, TColMember>)_refCols[fieldName];
    }
}
