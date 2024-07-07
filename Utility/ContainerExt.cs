
using Godot;

public static class ContainerExt
{
    public static T MakeContainer<T>(this Control c)
            where T : Container, new()
    {
        c.FullRect();
        c.MouseFilter = Control.MouseFilterEnum.Stop;
        var inner = new T();
        inner.FullRect();
        c.AddChild(inner);
        return inner;
    }
    public static T MakeScroll<T>(this Control c)
            where T : Container, new()
    {
        var scroll = new ScrollContainer();
        c.AddChild(scroll);
        var inner = new T();
        scroll.AddChild(inner);
        
        c.MouseFilter = Control.MouseFilterEnum.Stop;
        c.GuiInput += e =>
        {
            scroll._GuiInput(e);
            c.GetViewport().SetInputAsHandled();
        };
        
        return inner;
    }
}