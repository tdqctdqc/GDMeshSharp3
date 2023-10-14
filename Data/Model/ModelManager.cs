using System;
using System.Collections.Generic;
using System.Linq;

public class ModelManager<T> : IModelManager<T> where T : IModel
{
    public Dictionary<string, T> Models { get; }
    public ModelManager(ModelList<T> list)
    {
        var models = list.GetPropertiesOfType<T>();
        Models = models.ToDictionary(b => b.Name, b => b);
    }
}
