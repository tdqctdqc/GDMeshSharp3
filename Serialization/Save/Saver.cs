using System;
using System.Collections.Generic;
using System.Linq;
using Godot;

public class Saver
{
    public static void Save(Data data)
    {
        var file = SaveFile.Save(data);
        var bytes = Game.I.Serializer.MP.Serialize(file);
        var dir = DirAccess.Open("");
        var fileAccess = FileAccess.Open("save.sv", FileAccess.ModeFlags.Write);
        fileAccess.StoreBuffer(bytes);
        fileAccess.Close();
    }

    public static void Load()
    {
        var fileAccess = FileAccess.Open("save.sv", FileAccess.ModeFlags.Read);
        var bytes = fileAccess.GetBuffer((long)fileAccess.GetLength());
        fileAccess.Close();
        var saveFile = Game.I.Serializer.MP.Deserialize<SaveFile>(bytes);
        var data = new Data();
        var entities = saveFile.Entities
            .Select(eBytes =>
            {
                var u = Game.I.Serializer.MP.Deserialize<EntityCreationUpdate>(eBytes);
                var e = (Entity)Game.I.Serializer.MP.Deserialize(u.EntityBytes, u.EntityType);
                return e;
            }).ToList();
        data.AddEntities(entities, null);
        data.Notices.FinishedStateSync.Invoke();
        Game.I.StartHostSession(data);
    }
}