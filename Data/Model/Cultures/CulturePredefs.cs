using System;
using System.Collections.Generic;
using System.Linq;
using Godot;

public class CulturePredefs : IModelPredefs<Culture>
{
    public List<Culture> Cultures { get; private set; }
    public CulturePredefs()
    {
        Cultures = FileLoader<Culture>.LoadFromJson("Assets/Cultures/Cultures/",
                ".json", json => new Culture(json))
            .ToList();
    }
}
