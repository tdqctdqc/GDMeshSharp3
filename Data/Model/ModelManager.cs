using System;
using System.Collections.Generic;
using System.Linq;

public class ModelManager<T> : IModelManager<T> where T : IModel
{
    public ModelManager()
    {
    }
}
