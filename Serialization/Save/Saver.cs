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

    public static void Test(Data data)
    {
        // TestWaypointPolymorph(data);
        // TestNav(data);
        TestMock(data);
    }

    private static void TestWaypointPolymorph(Data data)
    {
        var nav = data.Planet.Nav;
        var kvp = nav.Waypoints.First();
        var wp = kvp.Value;
        var deserialized = SerializeAndDeserialize(wp, data);

        
        var nWp = deserialized;
        if (wp.Waypoint().GetType() != nWp.Waypoint().GetType())
        {
            GD.Print($"{wp.Waypoint().GetType()} {nWp.Waypoint().GetType()}");
        }
    }

    private static void TestNav(Data data)
    {
        var nav = data.Planet.Nav;
        var navDeserialized = SerializeAndDeserialize(nav, data);
        if (navDeserialized == null) throw new Exception();
        if (navDeserialized.Waypoints == null) throw new Exception();

        var kvp = nav.Waypoints.First();
        var wp = kvp.Value;
        var nWp = navDeserialized.Waypoints[kvp.Key];
        if (nWp == null) throw new Exception();
    }

    
    private static void TestMock(Data data)
    {
        var nav = data.Planet.Nav;
        var kvp = nav.Waypoints.First();
        var wp = kvp.Value.Waypoint();
        var mock = Mock<Waypoint>.Construct(wp, 27);
        var wpSerialized = data.Serializer.MP.Serialize(wp);
        var wpJson = MessagePackSerializer.ConvertToJson(wpSerialized);
        GD.Print("json: " + wpJson);
        
        var mockSerialized = data.Serializer.MP.Serialize(mock);
        var mockDeserialized = SerializeAndDeserialize(mock, data);
        
        var mockJson = MessagePackSerializer.ConvertToJson(mockSerialized);
        GD.Print("mock json: " + mockJson);
    }
    private static T SerializeAndDeserialize<T>(T t, Data data)
    {
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
