using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Godot;

public abstract class Message
{
    public byte[] Serialize(Data data)
    {
        return data.Serializer.MP.Serialize(this, GetType());
    }
}
