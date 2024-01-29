using System;
using System.Collections.Generic;
using System.Linq;
using Godot;
using MessagePack;
using MessagePack.Formatters;

public class EntityFormatter<T> : IMessagePackFormatter<T> where T : Entity
{
    private Data _data;

    public EntityFormatter(Data data)
    {
        _data = data;
    }
    public void Serialize(ref MessagePackWriter writer, T value, MessagePackSerializerOptions options)
    {
        if (value == null)
        {
            throw new Exception();
        }
        writer.WriteInt32(value.Id);
    }

    public T Deserialize(
        ref MessagePackReader reader, MessagePackSerializerOptions options)
    {
        if (reader.TryReadNil())
        {
            throw new Exception();
        }
        int id = reader.ReadInt32();
        return (T)_data[id];
    }
}
