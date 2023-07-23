using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using MessagePack;

public class Serializer
{
    public IReadOnlyHash<Type> ConcreteEntityTypes { get; private set; }
    public MessagePackManager MP { get; private set; }
    public Dictionary<string, Type> Types { get; private set; }
    public Dictionary<Type, IEntityMeta> _entityMetas;
    public IEntityMeta GetEntityMeta(Type type)
    {
        if (_entityMetas.ContainsKey(type) == false)
        {
            AddEntityMeta(type);
        }
        return _entityMetas[type];
    }
    public EntityMeta<T> GetEntityMeta<T>() where T : Entity
    {
        return (EntityMeta<T>)_entityMetas[typeof(T)];
    }
    
    public Serializer()
    {
        MP = new MessagePackManager();
        MP.Setup();
        SetupEntityMetas();
    }
    private void SetupEntityMetas()
    {
        Types = new Dictionary<string, Type>();
        _entityMetas = new Dictionary<Type, IEntityMeta>();
    }

    private void AddEntityMeta(Type entityType)
    {
        var metaType = typeof(EntityMeta<>);
        Types.Add(entityType.Name, entityType);
        var genericMeta = metaType.MakeGenericType(entityType);
        var constructor = genericMeta.GetConstructors()[0];
        var meta = constructor.Invoke(new object[]{});
        _entityMetas.Add(entityType, (IEntityMeta)meta);
    }

    public void ClearMetas()
    {
        
    }
    public bool Test(Data data)
    {
        var res = true;
        foreach (var valueRepo in data.Registers)
        {
            var e = valueRepo.Value.Entities.FirstOrDefault();
            var meta = data.Serializer.GetEntityMeta(e.GetType());
            if(e != null)
            {
                GD.Print("testing " + e.GetType());
                res = res && meta.TestSerialization(e, data);
            }
            else
            {
                GD.Print($"no {valueRepo.Key} to test");
            }
        }

        return res;
    }

        
}



