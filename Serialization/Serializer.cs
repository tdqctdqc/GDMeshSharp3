using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using MessagePack;

public class Serializer
{
    public MessagePackManager MP { get; private set; }
    
    public Serializer()
    {
        MP = new MessagePackManager();
        MP.Setup();
    }

    public bool Test(Data data)
    {
        var res = true;
        foreach (var node in data.GetAllEntityTypeNodes())
        {
            var eType = node.EntityType;
            var e = node.GetEntities().FirstOrDefault();
            var meta = node.Meta;
            if(e != null)
            {
                res = res && meta.TestSerialization(e, data);
            }
            else
            {
            }
        }

        return res;
    }
}



