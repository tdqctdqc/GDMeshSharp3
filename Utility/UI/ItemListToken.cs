
using System;
using System.Collections.Generic;
using System.Linq;
using Godot;

public class ItemListToken<T>
{
    public T Selected { get; private set; }
    public ItemList ItemList { get; private set; }
    private List<T> _items;
    
    public ItemListToken(
        IEnumerable<T> items, 
        Func<T, string> getLabelText, 
        Action<T> selectAction,
        Func<T, Texture2D> getTexture = null,
        Vector2I? textureSize = null)
    {
        ItemList = new ItemList();
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

        ItemList.ItemSelected += i =>
        {
            var selected = _items[(int)i];
            Selected = selected;
            selectAction(selected);
        };
    }
}
