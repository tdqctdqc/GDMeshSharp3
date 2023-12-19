using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using MessagePack;
using MessagePack.Formatters;

public class Serializer
{
    public MessagePackManager MP { get; private set; }

    static Serializer()
    {
        CheckPolymorphs();
    }
    public Serializer()
    {
        MP = new MessagePackManager();
        MP.Setup();
    }

    

    public static void TestCustom(Data data)
    {
    }
    private static T SerializeAndDeserialize<T>(T t, Data data)
    {
        var serialized = data.Serializer.MP.Serialize<T>(t);
        return (T)data.Serializer.MP.Deserialize<T>(serialized);
    }

    private static void CheckPolymorphs()
    {
        var assembly = Assembly.GetExecutingAssembly();
        var polymorphTypes = assembly
            .GetTypesOfType<IPolymorph>()
            .Where(t => t.IsInterface == false);
        var parentPolymorphTypes = polymorphTypes
            .Where(t => 
                t.BaseType == null 
                || typeof(IPolymorph).IsAssignableFrom(t.BaseType) == false
            );
        foreach (var polymorphType in parentPolymorphTypes)
        {
            if (polymorphType.HasAttribute<UnionAttribute>() == false)
            {
                throw new Exception(polymorphType.Name 
                                    + " is missing union attribute");
            }
            var derived = polymorphType
                .GetDerivedTypes(polymorphTypes);
            if (derived.Any(d => d.HasAttribute<UnionAttribute>()))
            {
                GD.Print(polymorphType.Name + " has derived w union attribute");
                throw new Exception();
            }
            var concreteDerived = derived
                .Where(t => t.IsConcreteType());
            var unionAttributes = polymorphType
                .GetCustomAttributes()
                .WhereOfType<UnionAttribute>();
            
            foreach (var type in concreteDerived)
            {
                if (unionAttributes.Any(a => a.SubType == type) == false)
                {
                    GD.Print(polymorphType.Name 
                             + " is missing union attribute for "
                        + type.Name);
                    throw new Exception();
                }
            }
            var unionedTypes = unionAttributes
                .Select(u => u.SubType);

            if(unionedTypes.Count() != unionedTypes.Distinct().Count())
            {
                GD.Print(polymorphType.Name 
                         + " has repeated union attributes for some derived type");
                throw new Exception();
            }
            var ids = unionAttributes
                .Select(u => (int)u.Key).Distinct();
            if (ids.Count() != unionAttributes.Count())
            {
                GD.Print(polymorphType.Name + " has repeated union attribute ids");
                throw new Exception();
            }
        }
    }
    
}
