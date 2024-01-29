using System;
using System.Collections.Generic;
using System.Linq;
using Godot;
using MessagePack;
using MessagePack.Formatters;


public class ColorFormatter : IMessagePackFormatter<Godot.Color>
{
    public void Serialize(
        ref MessagePackWriter writer, Color value, MessagePackSerializerOptions options)
    {
        if (value == null)
        {
            writer.WriteNil();
            return;
        }

        var args = new float[]{value.R, value.G, value.B, value.A};
        writer.WriteArrayHeader(args.Length);

        for (var i = 0; i < args.Length; i++)
        {
            writer.Write(args[i]);
        }
    }

    public Color Deserialize(
        ref MessagePackReader reader, MessagePackSerializerOptions options)
    {
        if (reader.TryReadNil())
        {
            throw new Exception();
        }

        options.Security.DepthStep(ref reader);

        int count = reader.ReadArrayHeader();
        if (count != 4) throw new Exception();
        var floats = new float?[count];
        for (int i = 0; i < count; i++)
        {
            floats[i] = reader.ReadSingle();
        }

        if (floats.Any(f => f.HasValue == false))
        {
            throw new Exception();
        }
        reader.Depth--;
        return new Color(floats[0].Value, floats[1].Value, floats[2].Value, floats[3].Value);
    }
}

