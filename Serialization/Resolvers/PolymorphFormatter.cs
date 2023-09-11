
using System;
using System.Buffers;
using System.Reflection;
using Godot;
using MessagePack;
using MessagePack.Formatters;

public class PolymorphFormatter<T> : IMessagePackFormatter<T>
    where T : Polymorph
{
    public static PolymorphFormatter<T> Construct()
    {
        return new PolymorphFormatter<T>();
    }
    public void Serialize(ref MessagePackWriter writer, T value, MessagePackSerializerOptions options)
    {
        // GD.Print("writing");
        if (value == null)
        {
            throw new Exception();
        }
        
        var type = value.Value.GetType();
        // GD.Print($"serializing inner type as {type.Name}");
        var typeBytes = Game.I.Client.Key.Data.Serializer.MP
            .Serialize(type, typeof(Type));
        
        
        var objectBytes = Game.I.Client.Key.Data.Serializer.MP
            .Serialize(value.Value, type);
        writer.Write(typeBytes);
        writer.Write(objectBytes);
    }

    public T Deserialize(ref MessagePackReader reader, MessagePackSerializerOptions options)
    {
        GD.Print("reading");

        if (reader.TryReadNil())
        {
            throw new Exception();
        }
        
        var typeBytes = reader.ReadBytes().Value.ToArray();
        var type = Game.I.Client.Key.Data.Serializer.MP
            .Deserialize<Type>(typeBytes);

        GD.Print("deserialized type as " + type.Name);
        var objectBytes = reader.ReadBytes().Value.ToArray();
        var obj = Game.I.Client.Key.Data.Serializer.MP
            .Deserialize(objectBytes, type);
        
        
        return (T)typeof(T).GetMethod(nameof(WaypointPolymorph.Construct),
            BindingFlags.Static | BindingFlags.Public)
            .Invoke(null, new []{obj});
    }
}