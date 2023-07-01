using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Godot;

public class CultureLoader
{
    public static Dictionary<string, Culture> Setup()
    {
        var cultures = new Dictionary<string, Culture>();
        var culturePaths = GodotFileExt.GetAllFilePathsOfTypes("res://Assets/Cultures/Cultures/", 
            new List<string>{ ".json"});
        culturePaths.ForEach(path =>
        {
            var json = GodotFileExt.ReadFileAsString(path);
            var culture = new Culture(json);
            cultures.Add(culture.Name, culture);
        });
        return cultures;
    }

}
