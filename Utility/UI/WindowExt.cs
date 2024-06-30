using System;
using System.Collections.Generic;
using System.Linq;
using Godot;

public static class WindowExt
{
    public static void MakeHideable(this Window w)
    {
        w.CloseRequested += w.Hide;
        w.WindowInput += i =>
        {
            if (i is InputEventKey k && k.Keycode == Key.Escape)
            {
                w.Hide();
                w.GetViewport().SetInputAsHandled();
            }
        };
    }

    public static void MakeFreeable(this Window w)
    {
        w.CloseRequested += w.QueueFree;
        w.WindowInput += i =>
        {
            if (i is InputEventKey k && k.Keycode == Key.Escape)
            {
                w.QueueFree();
                w.GetViewport().SetInputAsHandled();
            }
        };
    }

    public static T MakeWholeWindowScrollContainer<T>(this Window w,
        Vector2I size)
        where T : Container, new()
    {
        var panel = new Panel();
        var t = panel.MakeScroll<T>(size);
        w.AddChild(panel);
        return t;
    }
}
