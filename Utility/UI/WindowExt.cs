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
}
