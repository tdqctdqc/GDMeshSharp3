using System;
using System.Collections.Generic;
using System.Linq;
using Godot;

public class CultureManager : IModelManager<Culture>
{
    public List<Culture> Models { get; private set; }
    public Dictionary<string, Culture> ByName { get; private set; }
    public CultureManager()
    {
        Models = FileLoader<Culture>.LoadFromJson("Assets/Cultures/Cultures/",
                ".json", json => new Culture(json))
            .ToList();
        ByName = Models.ToDictionary(m => m.Name, m => m);
    }
}
