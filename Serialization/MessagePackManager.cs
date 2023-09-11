using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Godot;
using MessagePack;
using MessagePack.Formatters;
using MessagePack.Resolvers;

public class MessagePackManager
{
    private MessagePackSerializerOptions _options;
    public byte[] Serialize<T>(T t)
    {
        return MessagePackSerializer.Serialize(t.GetType(), t, _options);
    }
    public byte[] Serialize(object t, Type type)
    {
        return MessagePackSerializer.Serialize(type, t, _options);
    }
    public T Deserialize<T>(byte[] bytes)
    {
        return MessagePackSerializer.Deserialize<T>(bytes, _options);
    }
    public object Deserialize(byte[] bytes, Type type)
    {
        return MessagePackSerializer.Deserialize(type, bytes, _options);
    }
    public void Setup()
    {
        var resolver = MessagePack.Resolvers.CompositeResolver.Create(
            // enable extension packages first
            GodotCustomResolver.Instance, 
            MessagePack.Resolvers.ContractlessStandardResolver.Instance,
            MessagePack.Resolvers.StandardResolver.Instance,
            // finally use standard (default) resolver
            StandardResolver.Instance
        );
        _options = MessagePackSerializerOptions.Standard.WithResolver(resolver);
        // Pass options every time or set as default
        // MessagePackSerializer.DefaultOptions = _options;
    }
    
}

//supported types
