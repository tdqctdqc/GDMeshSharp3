using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Godot;
using MessagePack;

public static class SerializeChecker<TEntity> where TEntity : Entity
{
    public static bool Test(TEntity e, IReadOnlyDictionary<string, IEntityVarMeta> varMetas, Data data)
    {
        var res = true;
        var eType = typeof(TEntity);
        foreach (var varMeta in varMetas.Values)
        {
            res = res && varMeta.Test(e, data);
        }
        res = res && TestConstructor(data);
        return res;
    }

    private static bool TestConstructor(Data data)
    {
        var eType = typeof(TEntity);
        var constructors = eType.GetConstructors();
        if (constructors.Length > 0)
        {
            throw new SerializationException(typeof(TEntity) + " has public constructor");
        }
        constructors = constructors
            .Union(eType.GetConstructors(BindingFlags.NonPublic | BindingFlags.Instance))
            .Where(con => con.HasAttribute<SerializationConstructorAttribute>() && con.IsPrivate)
            .ToArray();

        if (constructors.Count() == 0)
        {
            GD.Print(typeof(TEntity) + " has no valid constructors");
            var cs = eType.GetConstructors(BindingFlags.NonPublic | BindingFlags.Instance);
            GD.Print("non pub instance constructors " + cs.Count());
            
            throw new SerializationException(typeof(TEntity) + " has no constructors");
        }
        
        if (constructors.Count() > 1)
        {
            GD.Print();
            foreach (var con in constructors)
            {
                GD.Print(con.GetParameters().Select(p => p.Name + " " +  p.ParameterType.ToString()).ToArray());
            }
            throw new SerializationException(typeof(TEntity) + " has multiple constructors");
        }

        var c = constructors[0];
        var meta = data.GetEntityMeta(typeof(TEntity));
        var fields = meta.FieldNameList.ToDictionary(n => n, n => meta.FieldTypes[n]);
        var paramInfos = c.GetParameters().ToDictionary(pi => pi.Name, pi => pi.ParameterType);
        
        foreach (var kvp in paramInfos)
        {
            var paramName = kvp.Key;
            var paramType = kvp.Value;
            var capFirst = char.ToUpper(paramName[0]) + paramName.Substring(1);
            if (fields.ContainsKey(capFirst) == false)
            {
                throw new SerializationException($"No matching var found for param {paramName} for {typeof(TEntity)}");
            }

            if (paramType != fields[capFirst])
            {
                throw new SerializationException($"Param type {paramType} is not the same as var type {fields[capFirst]} " +
                                                 $"for param {paramName} for {typeof(TEntity)}");
            }
        }
        foreach (var kvp in fields)
        {
            var fieldName = kvp.Key;
            var fieldType = kvp.Value;
            var minFirst = char.ToLower(fieldName[0]) + fieldName.Substring(1);
            if (paramInfos.ContainsKey(minFirst) == false)
            {
                GD.Print("Params ");
                foreach (var keyValuePair in paramInfos)
                {
                    GD.Print(keyValuePair.Key);
                }
                GD.Print($"No matching param found for var {minFirst} for {typeof(TEntity)}");
                throw new SerializationException($"No matching param found for var {minFirst} for {typeof(TEntity)}");
            }

            if (fieldType != paramInfos[minFirst])
            {
                throw new SerializationException($"Param type {fieldType} is not the same as var type {fields[minFirst]} " +
                                    $"for param {fieldName} for {typeof(TEntity)}");
            }
        }

        return true;
    }
}
