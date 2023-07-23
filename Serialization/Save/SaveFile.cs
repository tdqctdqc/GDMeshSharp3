using System;
using System.Collections.Generic;
using System.Linq;
using MessagePack;

public class SaveFile
{
    public List<byte[]> Entities { get; private set; }

    public static SaveFile Save(Data data)
    {
        return new SaveFile(data.EntitiesById.Values
            .Select(e => EntityCreationUpdate.Create(e, null))
            .Select(e => data.Serializer.MP.Serialize(e))
            .ToList());
    }
    [SerializationConstructor] public SaveFile(List<byte[]> entities)
    {
        Entities = entities;
    }
}
