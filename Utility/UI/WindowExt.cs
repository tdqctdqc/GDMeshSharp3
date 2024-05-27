using System;
using System.Collections.Generic;
using System.Linq;
using Godot;

public static class WindowExt
{
    public static void MakeCloseable(this Window w)
    {
        w.CloseRequested += w.Hide;
    }

    public static T MakeScrollContainer<T>(this Window w,
        Vector2I size)
        where T : Container, new()
    {
        var t = new T();
        var panel = new Panel();
        panel.MakeScroll<T>(size);
        w.AddChild(panel);
        return t;
    }
}
