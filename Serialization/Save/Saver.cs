using System;
using System.Collections.Generic;
using System.Linq;
using Godot;
using MessagePack;

public class Saver
{
    public static void Save(Data data)
    {
        var file = SaveFile.Save(data);
        
        
        var loaded = file.Entities
            .Select(eBytes =>
            {
                var u = data.Serializer.MP
                    .Deserialize<PolymorphMessage<Entity>>(eBytes);
                var e = (Entity)data.Serializer.MP.Deserialize(u.Bytes, u.Type);
                return e;
            }).ToList();

        if (loaded.Count() != data.EntitiesById.Count())
        {
            GD.Print($"{data.EntitiesById.Count()} entities became {loaded.Count()}");
        }

        var loadedIds = loaded.Select(l => l.Id).Distinct();
        if (loadedIds.Count() != loaded.Count())
        {
            GD.Print($"{loaded.Count()} with {loadedIds.Count()} distinct ids");
        }
        
        foreach (var loadedEntity in loaded)
        {
            var entity = data[loadedEntity.Id];
            if (entity.GetType() != loadedEntity.GetType())
            {
                GD.Print($"{entity.GetType()} {entity.Id} became {loadedEntity.GetType()}");
            }
        }
        
        
        
        GodotFileExt.SaveFile(file, "", "save", ".sv", data);
    }

    
    public static void Load()
    {
        var fileAccess = FileAccess.Open("save.sv", FileAccess.ModeFlags.Read);
        var bytes = fileAccess.GetBuffer((long)fileAccess.GetLength());
        fileAccess.Close();
        var data = new Data();
        if (data.EntitiesById.Count > 0) throw new Exception();
        var saveFile =
            GodotFileExt.LoadFileAs<SaveFile>("res://", "save", ".sv", data); 

        var entities = saveFile.Entities
            .Select(eBytes =>
            {
                var u = data.Serializer.MP
                    .Deserialize<PolymorphMessage<Entity>>(eBytes);
                var e = (Entity)data.Serializer.MP.Deserialize(u.Bytes, u.Type);
                return e;
            }).ToList();
        data.LoadEntities(entities, null);
        data.Notices.FinishedStateSync.Invoke();
        Game.I.LoadHostSession(data);
    }
}
