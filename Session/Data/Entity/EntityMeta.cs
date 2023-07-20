using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

public class EntityMeta<TEntity> : IEntityMeta where TEntity : Entity
{
    public Type EntityType => typeof(TEntity);
    public Type DomainType { get; private set; }
    public IReadOnlyList<string> FieldNameList => _fieldNames;
    public HashSet<string> FieldNameHash { get; }
    private List<string> _fieldNames;
    public IReadOnlyDictionary<string, Type> FieldTypes => _fieldTypes;

    private Dictionary<string, Type> _fieldTypes;
    public IReadOnlyDictionary<string, IEntityVarMeta> Vars => _vars;
    
    private Dictionary<string, IEntityVarMeta> _vars;
    private Dictionary<string, IRefColMeta> _refCols;

    public void ForReference()
    {
        return;
        new EntityMeta<TEntity>();
    }
    public EntityMeta()
    {
        var entityType = typeof(TEntity);
        //bc with generic parameters it will not capture all the classes
        if (entityType.ContainsGenericParameters)
        {
            throw new SerializationException($"Entity {entityType.Name} cannot have generic parameters");
        }
        
        DomainType = (Type)entityType
            .GetMethod(nameof(DomainType), BindingFlags.Static | BindingFlags.NonPublic)
            .Invoke(null, null);
        
        var properties = entityType.GetProperties(BindingFlags.Instance | BindingFlags.Public);
        _fieldNames = properties.Select(p => p.Name).ToList();
        FieldNameHash = _fieldNames.ToHashSet();
        _fieldTypes = properties.ToDictionary(p => p.Name, p => p.PropertyType);
        _vars = new Dictionary<string, IEntityVarMeta>();
        _refCols = new Dictionary<string, IRefColMeta>();
        
        var makeFuncsMi = typeof(EntityMeta<TEntity>).GetMethod(nameof(MakeFuncs),
            BindingFlags.Instance | BindingFlags.NonPublic);
        foreach (var propertyInfo in properties)
        {
            var makeFuncsGeneric = makeFuncsMi.MakeGenericMethod(propertyInfo.PropertyType);
            makeFuncsGeneric.Invoke(this, new []{propertyInfo});
        }
    }
    private void MakeFuncs<TProperty>(PropertyInfo prop)
    {
        var name = prop.Name;
        var type = prop.PropertyType;

        var mi = GetType().GetMethod(nameof(SetupVarType), BindingFlags.Instance | BindingFlags.NonPublic);
        var genericMi = mi.MakeGenericMethod(new[] {typeof(TProperty)});
        genericMi.Invoke(this, new []{prop});
    }
    private void SetupVarType<TProperty>(PropertyInfo prop)
    {
        var eVar = new EntityVarMeta<TEntity, TProperty>(prop);
        _vars.Add(prop.Name, eVar);
        var propType = typeof(TProperty);
        if (propType.IsGenericType && propType.GetGenericTypeDefinition().IsAssignableFrom(typeof(EntityRefCollection<>)))
        {
            // GD.Print(prop.Name);
            // GD.Print(typeof(TProperty));
            var genericParam = propType.GenericTypeArguments[0];
            var mi = GetType().GetMethod(nameof(SetupColType), 
                BindingFlags.Instance | BindingFlags.NonPublic);
            var genericMi = mi.MakeGenericMethod(new[] {genericParam});
            genericMi.Invoke(this, new []{prop});
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
    public IRefCollection<TKey> GetRefCollection<TKey>(string fieldName, Entity t, ProcedureWriteKey key)
    {
        return (IRefCollection<TKey>)_vars[fieldName].GetForSerialize((TEntity)t);
    }
    public void UpdateEntityVar<TProperty>(string fieldName, Entity t, StrongWriteKey key, TProperty newValue)
    {
        _vars[fieldName].UpdateVar(fieldName, t, key, newValue);
    }
    public EntityVarMeta<TEntity, TProperty> GetEntityVarMeta<TProperty>(string fieldName)
    {
        return (EntityVarMeta<TEntity, TProperty>)_vars[fieldName];
    }

    IRefColMeta<TColMember> IEntityMeta.GetRefColMeta<TColMember>(string fieldName)
    {
        return GetRefColMeta<TColMember>(fieldName);
    }
    public RefColMeta<TEntity, TColMember> GetRefColMeta<TColMember>(string fieldName)
    {
        return (RefColMeta<TEntity, TColMember>)_refCols[fieldName];
    }
    public bool TestSerialization(Entity e)
    {
        var t = (TEntity) e;
        return SerializeChecker<TEntity>.Test(t, _vars);
    }
}
