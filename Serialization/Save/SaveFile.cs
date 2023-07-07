using System;
using System.Collections.Generic;
using System.Linq;
using MessagePack;

public class SaveFile
{
    public List<byte[]> Entities { get; private set; }

    public static SaveFile Save(Data data)
    {
        return new SaveFile(data.Entities.Values
            .Select(e => EntityCreationUpdate.Create(e, null))
            .Select(e => Game.I.Serializer.MP.Serialize(e))
            .ToList());
    }
    [SerializationConstructor] public SaveFile(List<byte[]> entities)
    {
        Entities = entities;
    }
}
