using System;
using System.Collections.Generic;
using System.Linq;
using Godot;

public partial class TabWindow : Window
{
    protected TabContainer _container;
    protected List<IUiDrawable> _tabs;
    public TabWindow(Client c)
    {
        this.MakeFreeable();
        _container = new TabContainer();
        _container.AnchorsPreset = (int)Control.LayoutPreset.FullRect;
        _container.SetAnchorsPreset(Control.LayoutPreset.FullRect);
        AddChild(_container);
        _tabs = new List<IUiDrawable>();
        _container.TabSelected += i =>
        {
            _tabs[(int)i].Draw(c);
        };
        AboutToPopup += () => DrawSelected(c);
    }

    private TabWindow()
    {
    }

    private void DrawSelected(Client c)
    {
        _tabs[_container.CurrentTab].Draw(c);
    }
    protected void AddTab<T>(T tab)
            where T : Control, IUiDrawable
    {
        tab.AnchorsPreset = (int)Control.LayoutPreset.FullRect;
        _container.AddChild(tab);
        _tabs.Add(tab);
    }

    protected void Clear()
    {
        _container.ClearChildren();
        _tabs.Clear();
    }
}
