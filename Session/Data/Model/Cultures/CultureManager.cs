using System;
using System.Collections.Generic;
using System.Linq;
using Godot;

public class CultureManager : IModelManager<Culture>
{
    public Dictionary<string, Culture> Models { get; private set; }
    
    public CultureManager()
    {
        Models = FileLoader<Culture>.Setup("res://Assets/Cultures/Cultures/",
                ".json", json => new Culture(json))
            .ToDictionary(c => c.Name, c => c);
        
    }
}
