
using System;
using System.Collections.Generic;
using System.Linq;
using Godot;

public class ItemMultiListToken<T>
{
    public List<T> Selected { get; private set; }
    public ItemList ItemList { get; private set; }
    private List<T> _items;
    
    public ItemMultiListToken(
        IEnumerable<T> items, 
        Func<T, string> getLabelText, 
        Action<List<T>> selectAction,
        Vector2 size,
        Func<T, Texture2D> getTexture = null,
        Vector2I? textureSize = null)
    {
        Selected = new List<T>();
        ItemList = new ItemList();
        ItemList.CustomMinimumSize = size;
        ItemList.SelectMode = ItemList.SelectModeEnum.Multi;
        if (textureSize.HasValue)
        {
            ItemList.FixedIconSize = textureSize.Value;
        }

        _items = new List<T>();
        
        foreach (var item in items)
        {
            _items.Add(item);
            ItemList.AddItem(getLabelText(item),
                getTexture is not null
                    ? getTexture(item)
                    : null
            );
        }

        ItemList.MultiSelected += (index, selected) =>
        {
            HandleMultiSelection(selectAction);
        };
    }

    private void HandleMultiSelection(Action<List<T>> selectAction)
    {
        Selected = ItemList.GetSelectedItems()
            .Select(i => _items[i]).ToList();
        selectAction(Selected);
    }
}