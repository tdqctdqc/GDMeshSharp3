using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Godot;
using MessagePack;
using MessagePack.Formatters;

public class EntityResolver : IFormatterResolver
{
    // Resolver should be singleton.

    private Data _data;
    private Dictionary<Type, object> _formatters;
    public EntityResolver(Data data)
    {
        _data = data;
        var makeFormatterMi = GetType()
            .GetMethod(nameof(MakeFormatter), BindingFlags.NonPublic | BindingFlags.Instance);
        Func<Type, object> makeFormatter = t =>
        {
            return makeFormatterMi.MakeGenericMethod(t)
                .Invoke(this, new object[] { data });
        };
    }

    // GetFormatter<T>'s get cost should be minimized so use type cache.
    public IMessagePackFormatter<T> GetFormatter<T>()
    {
        return (IMessagePackFormatter<T>) _formatters[typeof(T)];
    }

    private IMessagePackFormatter<T> MakeFormatter<T>(Data data) where T : Entity
    {
        return new EntityFormatter<T>(data);
    }

}

