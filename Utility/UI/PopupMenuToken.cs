using System;
using System.Collections.Generic;
using Godot;


public class PopupMenuToken<T> where T : class
{
    public PopupMenu Menu { get; private set; }
    public T Selected { get; private set; }
    private List<T> _items;

    public PopupMenuToken(
        IEnumerable<T> items,
        Action<T> selectAction, 
        Func<T, Control> getControl)
    {
        Menu = new PopupMenu();
        _items = new List<T>();
        Selected = null;

        foreach (var item in items)
        {
            Menu.AddChild(getControl(item));
            _items.Add(item);
        }

        Menu.IndexPressed += i =>
        {
            var item = _items[(int)i];
            Selected = item;
            selectAction(item);
        };
    }
}