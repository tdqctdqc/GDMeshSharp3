using System;
using System.Collections.Generic;
using System.Linq;
using Godot;

public interface IModelManager<T> : IModelManager where T : IModel
{
    List<T> ExplicitModels { get; }
    Dictionary<string, T> ExplicitModelsByName { get; }
}

public interface IModelManager
{
    
}