
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
    public static T MakeScrollChild<T>(this Control c,
        out ScrollContainer scroll)
            where T : Container, new()
    {
        var s = new ScrollContainer();
        scroll = s;
        c.AddChild(scroll);
        var inner = new T();
        scroll.AddChild(inner);
        
        c.MouseFilter = Control.MouseFilterEnum.Stop;
        c.GuiInput += e =>
        {
            s._GuiInput(e);
            c.GetViewport().SetInputAsHandled();
        };
        
        return inner;
    }
    
    public static T MakeScroll<T>(out ScrollContainer scroll)
        where T : Container, new()
    {
        var s = new ScrollContainer();
        scroll = s;
        var inner = new T();
        scroll.AddChild(inner);
        
        return inner;
    }
}