
using Godot;

public static class ContainerExt
{
    public static T MakeScroll<T>(this Control c,
        Vector2 size)
            where T : Container, new()
    {
        c.CustomMinimumSize = size;
        c.Size = size;
        c.AnchorsPreset = (int)Control.LayoutPreset.FullRect;
        var Scroll = new ScrollContainer();
        Scroll.AnchorsPreset =  (int)Control.LayoutPreset.FullRect;
        c.MouseFilter = Control.MouseFilterEnum.Stop;
        Scroll.CustomMinimumSize = size;
        c.AddChild(Scroll);
        var Inner = new T();
        Inner.AnchorsPreset = (int)Control.LayoutPreset.FullRect;
        Inner.CustomMinimumSize = size;
        Scroll.AddChild(Inner);
        c.GuiInput += e =>
        {
            Scroll._GuiInput(e);
            c.GetViewport().SetInputAsHandled();
        };
        return Inner;
    }
}