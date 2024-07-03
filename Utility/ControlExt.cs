using Godot;
public static class ControlExt
{
    public static void ExpandFill(this Control c)
    {
        c.SizeFlagsHorizontal = Control.SizeFlags.ExpandFill;
        c.SizeFlagsVertical = Control.SizeFlags.ExpandFill;
    }

    public static void FullRect(this Control c)
    {
        c.AnchorsPreset = (int)Control.LayoutPreset.FullRect;
    }
}