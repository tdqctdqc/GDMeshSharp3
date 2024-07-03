using System;
using System.Linq;
using Godot;

public partial class SelectableControl : Panel
{
    public Control Parent { get; private set; }
    private Color _selectMod = new Color(.5f, .5f, .5f, .5f);
    public event Action Selected;
    public SelectableControl(Control parent)
    {
        Parent = parent;
        Parent.GuiInput += _GuiInput;
        foreach (var descendent in parent.GetDescendents().OfType<Control>())
        {
            descendent.MouseFilter = MouseFilterEnum.Pass;
        }
        parent.AddChild(this);
        MouseFilter = MouseFilterEnum.Stop;
        Modulate = Colors.Red;
    }

    public void Select()
    {
        ZIndex = 0;
        ZAsRelative = true;
        Modulate = _selectMod;
        Size = Parent.Size;
        Position = Vector2.Zero;
        Selected?.Invoke();
    }

    public void Deselect()
    {
        Modulate = Colors.Transparent;
    }

    public override void _GuiInput(InputEvent e)
    {
        if (e is InputEventMouseButton mb
            && mb.ButtonIndex == MouseButton.Left
            && mb.Pressed == false)
        {
            GetViewport().SetInputAsHandled();
            Select();
        }
    }
}