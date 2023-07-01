using System;
using System.Collections.Generic;
using System.Linq;
using Godot;

public static class SceneManager
{
    public static Dictionary<string, PackedScene> Scenes { get; private set; }

    public static void Setup()
    {
        Scenes = new Dictionary<string, PackedScene>();
        var scenePaths = GodotFileExt.GetAllFilePathsOfType("res://Assets/Scenes/", ".tscn");
        scenePaths.ForEach(s =>
        {
            var packed = GD.Load<PackedScene>(s);
            var inst = packed.Instantiate();
            var scriptName = inst.GetType().Name;
            inst.Free();
            Scenes.Add(scriptName, packed);
        });
    }
    public static T Instance<T>() where T : Node
    {
        return Scenes[typeof(T).Name].Instantiate<T>();
    }
    
}