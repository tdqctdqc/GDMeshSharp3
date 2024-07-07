using System;
using Godot;
public static class ControlExt
{
    public static void ExpandFill(this Control c, int ratio = -1)
    {
        c.SizeFlagsHorizontal = Control.SizeFlags.ExpandFill;
        c.SizeFlagsVertical = Control.SizeFlags.ExpandFill;
        if (ratio > 0)
        {
            c.SizeFlagsStretchRatio = ratio;
        }
    }

    public static void FullRect(this Control c)
    {
        c.AnchorsPreset = (int)Control.LayoutPreset.FullRect;
    }

    public static void AddClickUpAction(this Control c, 
        MouseButton button,
        Action a)
    {
        c.GuiInput += e =>
        {
            if (e is InputEventMouseButton mb
                && mb.ButtonIndex == button
                && mb.Pressed == false)
            {
                c.GetViewport().SetInputAsHandled();
                a.Invoke();
            }
        };
    }
}