
using System.Collections.Generic;
using Godot;

public partial class DrawTabContainer : TabContainer, IUiDrawable
{
    protected List<IUiDrawable> _tabs;

    public DrawTabContainer(Client c)
    {
        this.FullRect();
        _tabs = new List<IUiDrawable>();
        TabSelected += i =>
        {
            _tabs[(int)i].Draw(c);
        };
    }
    public void Draw(Client c)
    {
        _tabs[CurrentTab].Draw(c);
    }
    public void AddTab<T>(T tab)
        where T : Control, IUiDrawable
    {
        tab.FullRect();
        tab.ExpandFill();
        AddChild(tab);
        _tabs.Add(tab);
    }

    public T OpenTab<T>()
    {
        var index = _tabs.FindIndex(t => t is T);
        CurrentTab = index;
        return (T)_tabs[index];
    }
    public void Clear()
    {
        this.ClearChildren();
        _tabs.Clear();
    }
}