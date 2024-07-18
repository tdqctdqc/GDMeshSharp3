using System;
using System.Collections.Generic;
using System.Linq;
using Godot;

public class CultureManager : IModelManager<Culture>
{
    public List<Culture> ExplicitModels { get; private set; }
    public Dictionary<string, Culture> ExplicitModelsByName { get; private set; }
    public CultureManager()
    {
        ExplicitModels = FileLoader<Culture>.LoadFromJson("Assets/Cultures/Cultures/",
                ".json", json => new Culture(json))
            .ToList();
        ExplicitModelsByName = ExplicitModels.ToDictionary(m => m.Name, m => m);
    }
}
