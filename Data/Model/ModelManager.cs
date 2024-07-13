using System;
using System.Collections.Generic;
using System.Linq;

public class ModelManager<T> : IModelManager<T> where T : IModel
{
    public List<T> Models { get; }
    public Dictionary<string, T> ByName { get; private set; }
    
    public ModelManager()
    {
        Models = this.GetPropertiesOfType<T>();
        ByName = this.GetPropertiesOfTypeByName<T>();
    }
}
