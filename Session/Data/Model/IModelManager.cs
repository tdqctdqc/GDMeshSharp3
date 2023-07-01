using System;
using System.Collections.Generic;
using System.Linq;
using Godot;

public interface IModelManager<T> : IModelManager where T : IModel
{
    Dictionary<string, T> Models { get; }
}

public interface IModelManager
{
    
}