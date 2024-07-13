using System;
using System.Collections.Generic;
using System.Linq;
using Godot;

public interface IModelManager<T> : IModelManager where T : IModel
{
    List<T> Models { get; }
    Dictionary<string, T> ByName { get; }
}

public interface IModelManager
{
    
}