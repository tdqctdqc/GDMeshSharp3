using System;
using System.Collections.Generic;
using System.Linq;
using Godot;

public interface IIdentifiable
{
    int Id { get; }
}

public static class IIdentifiableExt
{
    public static Vector2 GetIdEdgeKey(this IIdentifiable i1, IIdentifiable i2)
    {
        var hi = i1.Id > i2.Id
            ? i1.Id
            : i2.Id;
        var lo = i1.Id > i2.Id
            ? i2.Id
            : i1.Id;
        return new Vector2(hi, lo);
    }
}
