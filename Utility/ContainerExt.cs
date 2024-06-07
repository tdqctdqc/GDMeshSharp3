
using Godot;

public static class ContainerExt
{
    public static T MakeContainer<T>(this Control c,
        Vector2 size)
            where T : Container, new()
    {
        c.CustomMinimumSize = size;
        c.AnchorsPreset = (int)Control.LayoutPreset.FullRect;
        c.MouseFilter = Control.MouseFilterEnum.Stop;
        var inner = new T();
        inner.AnchorsPreset = (int)Control.LayoutPreset.FullRect;
        c.AddChild(inner);
        return inner;
    }
    public static T MakeScroll<T>(this Control c,
        Vector2 size)
            where T : Container, new()
    {
        c.CustomMinimumSize = size;
        c.AnchorsPreset = (int)Control.LayoutPreset.FullRect;
        var scroll = new ScrollContainer();
        scroll.AnchorsPreset =  (int)Control.LayoutPreset.FullRect;
        c.MouseFilter = Control.MouseFilterEnum.Stop;
        scroll.CustomMinimumSize = size;
        c.AddChild(scroll);
        var inner = new T();
        inner.AnchorsPreset = (int)Control.LayoutPreset.FullRect;
        scroll.AddChild(inner);
        c.GuiInput += e =>
        {
            scroll._GuiInput(e);
            c.GetViewport().SetInputAsHandled();
        };
        return inner;
    }
}