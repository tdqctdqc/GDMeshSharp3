
using System;
using System.Collections.Generic;

public class FileLoader<T>
{
    public static List<T> Setup(string folder,
        string fileEnding, Func<string, T> construct)
    {
        var res = new List<T>();
        var paths = GodotFileExt.GetAllFilePathsOfTypes(folder, 
            new List<string>{ fileEnding });
        paths.ForEach(path =>
        {
            var json = GodotFileExt.ReadFileAsString(path);
            var t = construct(json);
            res.Add(t);
        });
        return res;
    }
}