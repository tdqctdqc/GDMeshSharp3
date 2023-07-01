
using System;
using System.Collections.Generic;
using System.Linq;
using Godot;

public partial class ItemListToken : Node
{
    private ItemList _node;
    private List<object> _items;
    private List<Action> _actions;

    public static ItemListToken Construct(ItemList node)
    {
        var token = new ItemListToken(node);
        node.AddChild(token);
        return token;
    }
    public void Setup<T>(IReadOnlyList<T> items, 
        Func<T, string> getLabelText, Func<T, Action> getAction)
    {
        _node.Clear();
        _items = items.Select(i => (object) i).ToList();
        _actions = items.Select(i => getAction(i)).ToList();

        var labelTexts = items.Select(i => getLabelText(i)).ToList();
        for (var i = 0; i < items.Count; i++)
        {
            _node.AddItem(getLabelText(items[i]));
        }
    }
    private ItemListToken(ItemList node)
    {
        _node = node;
        _node.AutoHeight = true;
        node.ItemSelected += i => SelectedCallback((int)i);
    }

    private void SelectedCallback(int i)
    {
        _actions[i]();
    }
}
