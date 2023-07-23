using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Godot;

public static class ReflectionExt 
{
    public static void InvokeGeneric(this object ob, string methodName, Type[] genericParams, object[] args)
    {
        var mi = ob.GetType().GetMethod(methodName, 
            BindingFlags.Instance | BindingFlags.NonPublic);
        var genericMi = mi.MakeGenericMethod(genericParams);
        genericMi.Invoke(ob, args);
    }
     public static List<T> GetStaticPropertiesOfType<T>(this Type type)
     {
         return type.GetProperties(BindingFlags.Static | BindingFlags.Public)
             .Where(p => typeof(T).IsAssignableFrom(p.PropertyType))
             .Select(p => (T)p.GetValue(null))
             .ToList();
     }
     public static List<T> GetPropertiesOfType<T>(this Type type, object instance)
     {
         return type.GetProperties(BindingFlags.Instance | BindingFlags.Public)
             .Where(p => typeof(T).IsAssignableFrom(p.PropertyType))
             .Select(p => (T)p.GetValue(instance))
             .ToList();
     }
    public static List<Type> GetConcreteTypesOfType<TAbstract>(this Assembly assembly)
    {
        return assembly.GetTypes()
            .Where(t => t.IsInterface == false && t.IsAbstract == false && typeof(TAbstract).IsAssignableFrom(t)).ToList();
    }
    public static List<Type> GetConcreteTypesOfType(this Assembly assembly, Type abstractType)
    {
        return assembly.GetTypes()
            .Where(t => t.IsInterface == false && t.IsAbstract == false && abstractType.IsAssignableFrom(t)).ToList();
    }

    public static List<Type> GetDirectlyDerivedTypes(this Type baseType, Type[] types)
    {
        return baseType.GetDerivedTypes(types).Where(t => t.BaseType == baseType).ToList();
    }
    public static List<Type> GetDerivedTypes(this Type baseType, Type[] types)
    {
        // Get all types from the given assembly
        List<Type> derivedTypes = new List<Type>();

        for (int i = 0, count = types.Length; i < count; i++)
        {
            Type type = types[i];
            if (IsSubclassOf(type, baseType))
            {
                // The current type is derived from the base type,
                // so add it to the list
                derivedTypes.Add(type);
            }
        }

        return derivedTypes;
    }

    public static bool IsSubclassOf(Type type, Type baseType)
    {
        if (type == null || baseType == null || type == baseType)
            return false;

        if (baseType.IsGenericType == false)
        {
            if (type.IsGenericType == false)
                return type.IsSubclassOf(baseType);
        }
        else
        {
            baseType = baseType.GetGenericTypeDefinition();
        }

        type = type.BaseType;
        Type objectType = typeof(object);

        while (type != objectType && type != null)
        {
            Type curentType = type.IsGenericType ?
                type.GetGenericTypeDefinition() : type;
            if (curentType == baseType)
                return true;

            type = type.BaseType;
        }

        return false;
    }
    public static bool HasAttribute<TAttribute>(this MemberInfo c) where TAttribute : Attribute
    {
        return c.GetCustomAttributesData().Any(d => d.AttributeType == typeof(TAttribute));
    }

    public static T MakeStaticMethodDelegate<T>(this MethodInfo m) where T : Delegate
    {
        return (T)Delegate.CreateDelegate(typeof(T), m);
    }

    public static Delegate MakeStaticMethodDelegate(this MethodInfo m, Type delegateType)
    {
        return Delegate.CreateDelegate(delegateType, m);
    }

    public static Type MakeCustomDelegateType(Type baseType, Type[] argTypes)
    {
        return baseType.MakeGenericType(argTypes);
    }
    public static T MakeInstanceMethodDelegate<T>(this MethodInfo m) where T : Delegate
    {
        return (T)Delegate.CreateDelegate(typeof(T), null, m);
    }

    public static Type GetMethodDelType(this MethodInfo mi)
    {
        return Delegate.CreateDelegate(null, mi).GetType();
    }
    public static Delegate MakeInstanceMethodDelegate(this MethodInfo m, Type delegateType)
    {
        return Delegate.CreateDelegate(delegateType, null, m);
    }
}