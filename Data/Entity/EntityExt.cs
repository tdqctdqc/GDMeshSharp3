using Godot;
using System;
using System.Collections.Generic;

public static class EntityExt
{
    public static ERef<T> MakeRef<T>(this T t) where T : Entity
    {
        return new ERef<T>(t);
    }
}
