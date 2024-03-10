using System;
using System.Collections.Generic;
using Godot;

public partial class MultiBar : Control
{
    private ButtonGroup _group;
    private List<Func<Control>> _controlFuncs;
    private Control _showing;
    private int _showingIndex = -1;
    
    private Container _buttonContainer;
    private Label _label;
    private Container _outerContainer;
    private MultiBar()
    {
    }

    public static MultiBar MakeVertical()
    {
        var mb = new MultiBar(new VBoxContainer(),
            new HBoxContainer());

        return mb;
    }
    public MultiBar(Container buttonContainer,
        Container outerContainer)
    {
        _label = new Label();
        _outerContainer = outerContainer;
        _outerContainer.AddChild(buttonContainer);
        _buttonContainer = buttonContainer;
        _buttonContainer.AddChild(_label);
        _controlFuncs = new List<Func<Control>>();
        _group = new ButtonGroup();
        _group.AllowUnpress = true;
        AddChild(_outerContainer);
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
        _buttonContainer.AddChild(button);
    }
    private void Show(int index)
    {
        if (_showingIndex != -1)
        {
            Hide(_showingIndex);
        }
        _showingIndex = index;
        _showing = _controlFuncs[index]();
        _outerContainer.AddChild(_showing);
    }

    private void Hide(int index)
    {
        if (_showingIndex == index)
        {
            _showingIndex = -1;
            _showing?.QueueFree();
        }
    }

    public void SetLabel(string text)
    {
        _label.Text = text;
    }
}