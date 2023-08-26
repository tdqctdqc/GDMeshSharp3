using System;
using System.Collections.Generic;
using System.Linq;
using Godot;

public partial class TabWindow : ClosableWindow
{
    private TabContainer _container;

    public TabWindow()
    {
        _container = new TabContainer();
        _container.AnchorsPreset = (int)Control.LayoutPreset.FullRect;
        _container.SetAnchorsPreset(Control.LayoutPreset.FullRect);
        AddChild(_container);
    }

    protected void AddTab(Control tab)
    {
        tab.AnchorsPreset = (int)Control.LayoutPreset.FullRect;
        _container.AddChild(tab);
    }

    protected void Clear()
    {
        _container.ClearChildren();
    }
}
