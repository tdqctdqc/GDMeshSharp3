using System;
using System.Collections.Generic;
using Godot;

public class SelectableControlListToken<T>
{
    public SelectableControlList Node { get; private set; }
    public T Value { get; private set; }
    public event Action<T> JustSelected;
    public IReadOnlyList<T> Items => _items;
    private List<T> _items;
    
    private Func<T, Control> _getControl;
    private Action<T> _selectAction;
    public SelectableControlListToken(Func<T, Control> getControl,
        Action<T> selectAction)
    {
        _getControl = getControl;
        _selectAction = selectAction;
        _items = new List<T>();
        Node = new SelectableControlList();
        Node.ItemSelected += HandleSelection;
    }
    
    public void Add(T t)
    {
        _items.Add(t);
        var control = _getControl(t);
        var entry = new SelectableControl(control);
        Node.Add(entry);
    }
    private void HandleSelection(int i)
    {
        var selected = _items[i];
        Value = selected;
        _selectAction(selected);
        JustSelected?.Invoke(selected);
    }
}