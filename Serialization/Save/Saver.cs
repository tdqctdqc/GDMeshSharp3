using System;
using System.Collections.Generic;
using System.Linq;
using Godot;

public class Saver
{
    public static void Save(Data data)
    {
        var file = SaveFile.Save(data);
        GodotFileExt.SaveFile(file, "", "save.sv", data);
    }

    public static void Load()
    {
        var fileAccess = FileAccess.Open("save.sv", FileAccess.ModeFlags.Read);
        var bytes = fileAccess.GetBuffer((long)fileAccess.GetLength());
        fileAccess.Close();
        var data = new Data();
        var saveFile =
            GodotFileExt.LoadFileAs<SaveFile>("", "save", ".sv", data); 

        var entities = saveFile.Entities
            .Select(eBytes =>
            {
                var u = data.Serializer.MP.Deserialize<EntityCreationUpdate>(eBytes);
                var e = (Entity)data.Serializer.MP.Deserialize(u.EntityBytes, u.EntityType);
                return e;
            }).ToList();
        data.AddEntities(entities, null);
        data.Notices.FinishedStateSync.Invoke();
        Game.I.LoadHostSession(data);
    }
}
