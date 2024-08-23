using System;
using System.Collections.Generic;
using Godot;

public partial class MultiBar : Control
{
    private ButtonGroup _group;
    private List<Button> _buttons;
    private Control _showing;
    private Label _label;
    private Container _container;
    private Vector2 _showingSize;
    

    public static MultiBar MakeVertical()
    {
        var mb = new MultiBar(new VBoxContainer(),
            new Vector2(300f, 600f));
        return mb;
    }
    public MultiBar(Container container, 
        Vector2 showingSize)
    {
        MouseFilter = MouseFilterEnum.Pass;
        _label = new Label();
        _showingSize = showingSize;
        _container = container;
        _container.MouseFilter = MouseFilterEnum.Pass;
        _buttons = new List<Button>();
        _group = new ButtonGroup();
        _group.AllowUnpress = true;
        AddChild(_container);
    }

    public void Add(Action action, string name)
    {
        var button = ButtonExt.GetToggleButton(() =>
            {
                action();
            },
            () =>
            {
                Hide();
            });
        button.Text = name;
        button.ButtonGroup = _group;
        _buttons.Add(button);
        _container.AddChild(button);
    }
    public void ShowPanel(Control c)
    {
        HidePanel();
        _showing = c;
        _showing.CustomMinimumSize = _showingSize;
        _showing.Size = _showingSize;
        AddChild(_showing);
        _showing.Position = Vector2.Right * _container.Size.X;
    }

    public void HidePanel()
    {
        _showing?.QueueFree();
        _showing = null;
    }

    public void SetLabel(string text)
    {
        _label.Text = text;
    }
}