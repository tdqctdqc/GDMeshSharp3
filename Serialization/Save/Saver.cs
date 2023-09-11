using System;
using System.Collections.Generic;
using System.Linq;
using Godot;
using MessagePack;

public class Saver
{
    public static void Save(Data data, WriteKey key)
    {
        var file = SaveFile.Save(data, key);
        GodotFileExt.SaveFile(file, "", "save", ".sv", data);
    }

    public static void TestNav(Data data)
    {
        var nav = data.Planet.Nav;


        foreach (var wrapper in nav.Waypoints.Values)
        {
            var wpPolymorph = new WaypointPolymorph(wrapper.Value());
            var polymorphDeserialized = 
                SerializeAndDeserialize(wpPolymorph, data);
            if (wpPolymorph.Value.GetType() != polymorphDeserialized.Value.GetType())
            {
                GD.Print($"{wpPolymorph.Value.GetType()} " +
                         $"became {polymorphDeserialized.Value.GetType()}");
            }
        }
        // foreach (var wrapper in nav.Waypoints.Values)
        // {
        //     var polymorph = wrapper.Polymorph;
        //     var deserialized = SerializeAndDeserialize(polymorph, data);
        // }
    }

    private static T SerializeAndDeserialize<T>(T t, Data data)
    {
        GD.Print(t.GetType());
        var serialized = data.Serializer.MP.Serialize(t, t.GetType());
        return (T)data.Serializer.MP.Deserialize(serialized, t.GetType());
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
                var u = data.Serializer.MP.Deserialize<PolymorphMessage<Entity>>(eBytes);
                var e = (Entity)data.Serializer.MP.Deserialize(u.Bytes, u.Type);
                return e;
            }).ToList();
        data.LoadEntities(entities, null);
        data.Notices.FinishedStateSync.Invoke();
        Game.I.LoadHostSession(data);
    }
}
