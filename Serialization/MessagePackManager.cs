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
        try
        {
            return MessagePackSerializer.Serialize(t, _options);
        }
        catch (Exception e)
        {
            GD.Print();
            throw new SerializationException("couldnt serialize " + t.GetType());
        }
    }
    public byte[] Serialize(object t, Type type)
    {
        try
        {
            return MessagePackSerializer.Serialize(type, t, _options);
        }
        catch (Exception e)
        {
            GD.Print("couldnt serialize " + type);
            throw;
        }
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
            GodotCustomResolver.Instance, //need this one to make fe Color transfer lossless
            MessagePack.Resolvers.ContractlessStandardResolver.Instance,
            MessagePack.Resolvers.StandardResolverAllowPrivate.Instance,
            // finally use standard (default) resolver
            StandardResolver.Instance
        );
        _options = MessagePackSerializerOptions.Standard.WithResolver(resolver);
        // Pass options every time or set as default
        MessagePackSerializer.DefaultOptions = _options;
    }
    
}

//supported types
