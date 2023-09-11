using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Godot;

public class GodotFileExt
{
    public static void SaveFile<T>(T t, string path, string name, string ext, Data data)
    {
        var bytes = data.Serializer.MP.Serialize(t);
        var dir = DirAccess.Open(path);
        var fileAccess = FileAccess.Open(name+ext, FileAccess.ModeFlags.Write);
        fileAccess.StoreBuffer(bytes);
        fileAccess.Close();
    }
    public static T LoadFileAs<T>(string path, string name, string ext, Data data)
    {
        var filePaths = GetAllFilePathsOfType(path, ext);
        foreach (var filePath in filePaths)
        {
            var fileName = GetFileName(filePath);
            if (fileName == name)
            {
                var fullPath = path + name + ext;
                var f = FileAccess.Open(fullPath, FileAccess.ModeFlags.Read);
                var bytes = f.GetBuffer((long)f.GetLength());
                f.Close();
                return data.Serializer.MP.Deserialize<T>(bytes);
            }
        }
        throw new Exception($"couldn't find {name} in {path}");
    }
    public static List<string> GetAllFilePathsOfType(string path, string type)
    {
        var filePaths = new List<string>();
        var dir = DirAccess.Open(path);
        if (dir == null) return filePaths;
        dir.ListDirBegin();
        var filename = dir.GetNext();
        while(filename != "")
        {
            if (dir.CurrentIsDir() && filename.StartsWith(".") == false)
            {
                filePaths.AddRange(GetAllFilePathsOfType(path.PathJoin(filename), type));
            }
            else if(filename.EndsWith(type))
            {
                filePaths.Add(path.PathJoin(filename));
            }

            filename = dir.GetNext();
        }

        return filePaths;
    }
    
    
    public static List<string> GetAllFilePathsOfTypes(string path, List<string> types)
    {
        var filePaths = new List<string>();
        var dir = DirAccess.Open(path);
        if (dir == null) return filePaths;
        dir.ListDirBegin();
        var filename = dir.GetNext();
        while(filename != "")
        {
            if (dir.CurrentIsDir() && filename.StartsWith(".") == false)
            {
                filePaths.AddRange(GetAllFilePathsOfTypes(path.PathJoin(filename), types));
            }
            else if(types.Any(t => filename.EndsWith(t)))
            {
                filePaths.Add(path.PathJoin(filename));
            }

            filename = dir.GetNext();
        }

        return filePaths;
    }

    public static string ReadFileAsString(string path)
    {
        
        var f = FileAccess.Open(path, FileAccess.ModeFlags.Read);
        var sb = new StringBuilder();
        while (f.EofReached() == false)
        {
            sb.Append(f.GetLine());
        }
        return sb.ToString();
    }

    public static string GetFileName(string path)
    {
        var lastSlash = path.LastIndexOf("/");
        var period = path.LastIndexOf(".");
        var length = period - lastSlash - 1;
        return path.Substring(lastSlash + 1, length);
    }
}
