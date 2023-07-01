using System;
using System.Collections.Generic;
using System.Linq;
using Godot;
using MessagePack;
using MessagePack.Formatters;


public class Vector2Formatter : IMessagePackFormatter<Vector2>
{
    public void Serialize(
        ref MessagePackWriter writer, Vector2 value, MessagePackSerializerOptions options)
    {
        if (value == null)
        {
            writer.WriteNil();
            return;
        }
        
        writer.WriteArrayHeader(2);
        writer.Write(value.X);
        writer.Write(value.Y);
    }

    public Vector2 Deserialize(
        ref MessagePackReader reader, MessagePackSerializerOptions options)
    {
        if (reader.TryReadNil())
        {
            throw new Exception();
        }
        options.Security.DepthStep(ref reader);

        int count = reader.ReadArrayHeader();
        
        float? x = null;
        float? y = null;
        for (int i = 0; i < count; i++)
        {
            switch (i)
            {
                case 0:
                    x = reader.ReadSingle();
                    break;
                case 1:
                    y = reader.ReadSingle();
                    break;
                default:
                    reader.Skip();
                    break;
            }
        }

        if (x.HasValue == false || y.HasValue == false)
        {
            throw new Exception();
        }
        reader.Depth--;
        return new Vector2(x.Value, y.Value);
    }
}