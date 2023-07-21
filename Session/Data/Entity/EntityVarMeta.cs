using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Godot;

public class EntityVarMeta<TEntity, TProperty> : IEntityVarMeta<TEntity> where TEntity : Entity
{
    public string PropertyName { get; private set; }

    public ValChangeAction<TProperty> ValChanged()
    {
        if (_valChanged == null)
        {
            _valChanged = new ValChangeAction<TProperty>();
        }
        return _valChanged;
    }
    private ValChangeAction<TProperty> _valChanged;
    protected Func<TEntity, TProperty> GetProperty { get; private set; }
    protected Action<TEntity, TProperty> SetProperty { get; private set; }
    public EntityVarMeta(PropertyInfo prop)
    {
        PropertyName = prop.Name;
        var getMi = prop.GetGetMethod();
        if (getMi == null) throw new SerializationException($"No get method for {PropertyName}");
        GetProperty = getMi.MakeInstanceMethodDelegate<Func<TEntity, TProperty>>();
        
        var setMi = prop.GetSetMethod(true);
        if (setMi == null) throw new SerializationException($"No set method for {PropertyName}");
        SetProperty = setMi.MakeInstanceMethodDelegate<Action<TEntity, TProperty>>();
    }
    public object GetForSerialize(Entity e)
    {
        return GetProperty((TEntity)e);
    }
    public void UpdateVar(string fieldName, Entity t, StrongWriteKey key, object newValueOb)
    {
        var oldValue = (TProperty)GetForSerialize((TEntity)t);
        var newValue = (TProperty) newValueOb;
        SetProperty((TEntity)t, newValue);
        _valChanged?.Invoke(new ValChangeNotice<TProperty>(t, fieldName, 
            newValue, oldValue));
        if (key is HostWriteKey hKey)
        {
            var bytes = Game.I.Serializer.MP.Serialize(newValue);
            hKey.HostServer.QueueMessage(EntityVarUpdate.Create(fieldName, t.Id, bytes, hKey));
        }
    }
    public bool Test(Entity t)
    {
        var prop = GetProperty((TEntity)t);
        try
        {
            var bytes = Game.I.Serializer.MP.Serialize(prop);
            var deserialized = Game.I.Serializer.MP.Deserialize<TProperty>(bytes);
        }
        catch (Exception e)
        {
            GD.Print($"Couldn't serialize property {PropertyName} for {typeof(TEntity)}");
            throw e;
        }

        return true;
    }
}