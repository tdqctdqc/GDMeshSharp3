using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Godot;

public class EntityVarMeta<TEntity, TProperty> : IEntityVarMeta<TEntity> where TEntity : Entity
{
    public string PropertyName { get; private set; }
    protected Func<TEntity, TProperty> GetProperty { get; private set; }
    protected Action<TEntity, TProperty> SetProperty { get; private set; }
    public EntityVarMeta(PropertyInfo prop)
    {
        PropertyName = prop.Name;
        var getMi = prop.GetGetMethod();
        if (getMi == null)
        {
            GD.Print($"No get method for {PropertyName}");
            throw new SerializationException($"No get method for {PropertyName}");
        }

        try
        {
            GetProperty = getMi.MakeInstanceMethodDelegate<Func<TEntity, TProperty>>();

        }
        catch (Exception e)
        {
            GD.Print("couldnt make delegate for " + PropertyName);
            throw;
        }
        
        var setMi = prop.GetSetMethod(true);
        if (setMi == null)
        {
            GD.Print($"No set method for {PropertyName}");
            throw new SerializationException($"No set method for {PropertyName}");
        }
        SetProperty = setMi.MakeInstanceMethodDelegate<Action<TEntity, TProperty>>();
    }
    public object GetForSerialize(Entity e)
    {
        return GetProperty((TEntity)e);
    }
    public bool Test(Entity t, Data data)
    {
        var prop = GetProperty((TEntity)t);
        
        try
        {
            var bytes = data.Serializer.MP.Serialize(prop);
            var deserialized = data.Serializer.MP.Deserialize<TProperty>(bytes);
        }
        catch (Exception e)
        {
            GD.Print($"Couldn't serialize property {PropertyName} for {typeof(TEntity)}");
            throw e;
        }

        return true;
    }
}