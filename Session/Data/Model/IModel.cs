using System;
using System.Collections.Generic;
using System.Linq;
using Godot;

public interface IModel : IIdentifiable
{
    string Name { get; }
    IReadOnlyList<IModelAttribute> AttributeList { get; }
}

public static class IModelExt
{
    public static ModelRef<T> MakeRef<T>(this T model) where T : class, IModel
    {
        return new ModelRef<T>(model.Id);
    }

    public static TAttribute GetAttribute<TAttribute>(this IModel model)
    {
        return model.AttributeList
            .SelectWhereOfType<TAttribute>().First();
    }
}