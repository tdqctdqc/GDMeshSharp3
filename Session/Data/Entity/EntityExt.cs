using Godot;
using System;
using System.Collections.Generic;

public static class EntityExt
{
    public static EntityRef<T> MakeRef<T>(this T t) where T : Entity
    {
        return new EntityRef<T>(t);
    }
    public static Vector2 GetV2EdgeKey(this Entity p, Entity n)
    {
        var hi = p.Id > n.Id
            ? p.Id
            : n.Id;
        var lo = p.Id > n.Id
            ? n.Id
            : p.Id;
        return new Vector2(hi, lo);
    }
}
