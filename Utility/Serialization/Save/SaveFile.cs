using System;
using System.Collections.Generic;
using System.Linq;
using Godot;
using MessagePack;

public class SaveFile
{
    public List<byte[]> Entities { get; private set; }

    public static SaveFile Save(Data data)
    {
        var entities = data.EntitiesById.Values.ToList();
        var saveFile = new SaveFile(data.EntitiesById.Values
            .Select(e => PolymorphMessage<Entity>.Construct(e, data))
            .Select(m => data.Serializer.MP.Serialize(m))
            .ToList());

        var loaded = saveFile.Entities
            .Select(eBytes =>
            {
                var u = data.Serializer.MP.Deserialize<PolymorphMessage<Entity>>(eBytes);
                var e = (Entity)data.Serializer.MP.Deserialize(u.Bytes, u.Type);
                return e;
            }).ToList();
        if (loaded.Count != entities.Count) throw new Exception("diff number entities");
        for (var i = 0; i < entities.Count; i++)
        {
            var e = entities[i];
            var l = loaded[i];
            var oId = e.Id;
            var nId = l.Id;
            
            if (oId != nId)
            {
                GD.Print($"{e.GetType()} changed id from {oId} to {nId}");
            }

            if (e.GetType() != l.GetType())
            {
                GD.Print($"{e.GetType()} changed type from {e.GetType()} to {l.GetType()}");
            }
        }
        
        return saveFile;
    }

    
    [SerializationConstructor] public SaveFile(List<byte[]> entities)
    {
        Entities = entities;
    }
}
