using System;
using System.Collections.Generic;
using System.Linq;
using Godot;

public interface IModel
{
    string Name { get; }
    int Id { get; }
}

public static class IModelExt
{
    public static ModelRef<T> MakeRef<T>(this T model) where T : class, IModel
    {
        return new ModelRef<T>(model.Id);
    }
}