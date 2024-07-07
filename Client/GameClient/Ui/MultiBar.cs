using System;
using System.Collections.Generic;
using Godot;

public partial class MultiBar : Control
{
    private ButtonGroup _group;
    private List<Func<Control>> _controlFuncs;
    private List<Button> _buttons;
    private Control _showing;
    private int _showingIndex = -1;
    
    private Label _label;
    private Container _container;
    private Vector2 _showingSize;
    private MultiBar()
    {
    }

    public static MultiBar MakeVertical()
    {
        var mb = new MultiBar(new VBoxContainer(),
            new Vector2(300f, 600f));
        return mb;
    }
    public MultiBar(Container container, 
        Vector2 showingSize)
    {
        _label = new Label();
        _showingSize = showingSize;
        _container = container;
        _controlFuncs = new List<Func<Control>>();
        _buttons = new List<Button>();
        _group = new ButtonGroup();
        _group.AllowUnpress = true;
        AddChild(_container);
    }

    public void Add(Func<Control> func, string name)
    {
        var index = _controlFuncs.Count;
        _controlFuncs.Add(func);
        var button = ButtonExt.GetToggleButton(() =>
            {
                Show(index);
            },
            () =>
            {
                Hide(index);
            });
        button.Text = name;
        button.ButtonGroup = _group;
        _buttons.Add(button);
        _container.AddChild(button);
    }
    private void Show(int index)
    {
        if (_showingIndex != -1)
        {
            Hide(_showingIndex);
        }
        _showingIndex = index;
        _showing = _controlFuncs[index]();
        _showing.CustomMinimumSize = _showingSize;
        _showing.Size = _showingSize;
        AddChild(_showing);
        _showing.Position = Vector2.Right * _container.Size.X;
    }

    private void Hide(int index)
    {
        if (_showingIndex == index)
        {
            _showingIndex = -1;
            _showing?.QueueFree();
            _showing = null;
        }
    }

    public void SetLabel(string text)
    {
        _label.Text = text;
    }
}