using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

public class EntityMeta<TEntity> : IEntityMeta where TEntity : Entity
{
    public Type EntityType => typeof(TEntity);
    public IReadOnlyList<string> FieldNameList => _fieldNames;
    private List<string> _fieldNames;
    public IReadOnlyDictionary<string, Type> FieldTypes => _fieldTypes;

    private Dictionary<string, Type> _fieldTypes;
    
    private Dictionary<string, IEntityVarMeta> _vars;
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
        
        _fieldNames = properties.Select(p => p.Name).ToList();
        _fieldTypes = properties.ToDictionary(p => p.Name, p => p.PropertyType);
        _vars = new Dictionary<string, IEntityVarMeta>();
        _refCols = new Dictionary<string, IRefColMeta>();
        
        foreach (var propertyInfo in properties)
        {
            this.InvokeGeneric(nameof(MakeFuncs),
                new []{propertyInfo.PropertyType}, new []{propertyInfo});
        }
        
        var declaredProperties = entityType.GetProperties(BindingFlags.DeclaredOnly 
                                                          | BindingFlags.Instance | BindingFlags.Public);
        foreach (var declaredProperty in declaredProperties)
        {
            this.InvokeGeneric(nameof(SetupRefCol), 
                new []{declaredProperty.PropertyType}, 
                new []{declaredProperty});
        }
    }
    private void MakeFuncs<TProperty>(PropertyInfo prop)
    {
        var name = prop.Name;
        var type = prop.PropertyType;
        var eVar = new EntityVarMeta<TEntity, TProperty>(prop);
        _vars.Add(prop.Name, eVar);
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
    public object[] GetPropertyValues(Entity entity)
    {
        var t = (TEntity) entity;
        var args = new object[_fieldNames.Count];
        for (int i = 0; i < _fieldNames.Count; i++)
        {
            var fieldName = _fieldNames[i];
            args[i] = _vars[fieldName].GetForSerialize(t);
        }
        return args;
    }
    IRefColMeta<TColMember> IEntityMeta.GetRefColMeta<TColMember>(string fieldName)
    {
        return GetRefColMeta<TColMember>(fieldName);
    }
    public RefColMeta<TEntity, TColMember> GetRefColMeta<TColMember>(string fieldName)
    {
        return (RefColMeta<TEntity, TColMember>)_refCols[fieldName];
    }
    public bool TestSerialization(Entity e, Data data)
    {
        var t = (TEntity) e;
        return SerializeChecker<TEntity>.Test(t, _vars, data);
    }
}
