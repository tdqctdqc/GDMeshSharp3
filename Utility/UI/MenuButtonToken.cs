using System;
using System.Collections.Generic;
using Godot;


public class MenuButtonToken<T> where T : class
{
    public MenuButton Button { get; private set; }
    public T Selected { get; private set; }
    private List<T> _items;

    public MenuButtonToken(
        IEnumerable<T> items,
        Action<T> selectAction, 
        Func<T, Control> getControl)
    {
        Button = new MenuButton();
        _items = new List<T>();
        Selected = null;

        foreach (var item in items)
        {
            var c = getControl(item);
            Button.AddChild(c);
            _items.Add(item);
        }

        // Button.IndexPressed += i =>
        // {
        //     var item = _items[(int)i];
        //     Selected = item;
        //     selectAction(item);
        // };
    }
}