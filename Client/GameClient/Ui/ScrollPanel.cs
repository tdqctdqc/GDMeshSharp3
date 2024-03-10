using Godot;

public partial class ScrollPanel : Panel
{
    public ScrollContainer Scroll { get; private set; }
    public VBoxContainer Inner { get; private set; }
    protected ScrollPanel()
    {
    }

    public static ScrollPanel Construct(Control inner, 
        Vector2 size,
        Color? color = null)
    {
        var s = new ScrollPanel(size, color);
        s.Inner.AddChild(inner);
        return s;
    }
    public ScrollPanel(Vector2 size,
         Color? color = null)
    {
        if (color.HasValue)
        {
            SelfModulate = color.Value;
        }
        CustomMinimumSize = size;
        AnchorsPreset = (int)LayoutPreset.FullRect;
        Scroll = new ScrollContainer();
        Scroll.AnchorsPreset =  (int)LayoutPreset.FullRect;
        MouseFilter = MouseFilterEnum.Stop;
        Scroll.CustomMinimumSize = size;
        AddChild(Scroll);
        Inner = new VBoxContainer();
        Inner.AnchorsPreset = (int)LayoutPreset.FullRect;
        Inner.CustomMinimumSize = size;
        Scroll.AddChild(Inner);
    }
    
    public override void _GuiInput(InputEvent @event)
    {
        Scroll._GuiInput(@event);
        GetViewport().SetInputAsHandled();
    }
}