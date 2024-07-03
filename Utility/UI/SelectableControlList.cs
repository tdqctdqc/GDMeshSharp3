using System;
using System.Collections.Generic;
using System.Reflection.Metadata;
using Godot;

public partial class SelectableControlList : VBoxContainer
{
    public event Action<int> ItemSelected;
    public int SelectedIndex { get; private set; }
    private List<SelectableControl> _controls;

    public SelectableControlList()
    {
        _controls = new List<SelectableControl>();
        // AnchorsPreset = (int)LayoutPreset.HcenterWide;
        SelectedIndex = -1;
    }
    
    public void Add(SelectableControl c)
    {
        // c.ExpandFill();
        _controls.Add(c);
        AddChild(c.Parent);
        c.Selected += () =>
        {
            HandleSelection(c);
        };
        if (_controls.Count == 1)
        {
            c.Select();
        }
    }

    private void HandleSelection(SelectableControl c)
    {
        var i = _controls.IndexOf(c);
        if (i == -1) throw new Exception();
        SelectedIndex = i;
        for (var j = 0; j < _controls.Count; j++)
        {
            var child = _controls[j];
            if (j != i)
            {
                child.Deselect();
            }
        }
        
        ItemSelected?.Invoke(i);
    }
}