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
    public IEntityMeta GetEntityMeta(Type type) => _entityMetas[type];
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
        var reference = nameof(EntityMeta<Entity>.ForReference);
        _entityMetas = new Dictionary<Type, IEntityMeta>();
        var entityTypes = Assembly.GetExecutingAssembly().GetConcreteTypesOfType<Entity>();
        ConcreteEntityTypes = new ReadOnlyHash<Type>(new HashSet<Type>(entityTypes));
        var metaTypes = typeof(EntityMeta<>);
        foreach (var entityType in entityTypes)
        {
            Types.Add(entityType.Name, entityType);
            var genericMeta = metaTypes.MakeGenericType(entityType);
            var constructor = genericMeta.GetConstructors()[0];
            var meta = constructor.Invoke(new object[]{});
            _entityMetas.Add(entityType, (IEntityMeta)meta);
        }
    }

    public void ClearMetas()
    {
        
    }
    public bool Test(Data data)
    {
        var res = true;
        foreach (var keyValuePair in data.Domains)
        {
            foreach (var valueRepo in keyValuePair.Value.Registers)
            {
                var e = valueRepo.Value.Entities.FirstOrDefault();
                if(e != null)
                {
                    GD.Print("testing " + e.GetType());
                    res = res && e.GetMeta().TestSerialization(e);
                }
                else
                {
                    GD.Print($"no {valueRepo.Key} to test");
                }
            }
        }

        return res;
    }

        
}



