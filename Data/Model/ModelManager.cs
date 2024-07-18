using System;
using System.Collections.Generic;
using System.Linq;

public class ModelManager<T> : IModelManager<T> where T : IModel
{
    public List<T> ExplicitModels { get; }
    public Dictionary<string, T> ExplicitModelsByName { get; private set; }
    
    public ModelManager()
    {
        ExplicitModels = this.GetPropertiesOfType<T>();
        ExplicitModelsByName = this.GetPropertiesOfTypeByName<T>();
    }
}
