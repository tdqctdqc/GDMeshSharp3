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
    public static Vector2I GetIdEdgeKey(this IIdentifiable i1, IIdentifiable i2)
    {
        var hi = i1.Id > i2.Id
            ? i1.Id
            : i2.Id;
        var lo = i1.Id > i2.Id
            ? i2.Id
            : i1.Id;
        return new Vector2I(hi, lo);
    }
    public static Vector2I GetIdEdgeKey(this IIdentifiable i1, int i2)
    {
        var hi = i1.Id > i2
            ? i1.Id
            : i2;
        var lo = i1.Id > i2
            ? i2
            : i1.Id;
        return new Vector2I(hi, lo);
    }
    public static Vector2I GetIdEdgeKey(this int i1, int i2)
    {
        var hi = i1 > i2
            ? i1
            : i2;
        var lo = i1 > i2
            ? i2
            : i1;
        return new Vector2I(hi, lo);
    }
}
