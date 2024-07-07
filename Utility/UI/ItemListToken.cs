
using System;
using System.Collections.Generic;
using System.Linq;
using Godot;

public class ItemListToken<T>
{
    public T Value { get; private set; }
    public event Action<T> JustSelected;
    public ItemList ItemList { get; private set; }
    public IReadOnlyList<T> Items => _items;
    private List<T> _items;
    private Func<T, Texture2D> _getTexture;
    private Vector2I? _textureSize;
    private Func<T, string> _getLabelText;
    private Action<T> _selectAction;
    
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

        _items = items.ToList();
        _selectAction = selectAction;
        _getLabelText = getLabelText;
        _getTexture = getTexture;
        _textureSize = textureSize;
        
        SetList();
        ItemList.AllowReselect = true;
        ItemList.ItemSelected += i =>
        {
            HandleSelection();
        };
    }
    
    
    
    private void SetList()
    {
        foreach (var item in _items)
        {
            AddItemToList(item);
        }
    }

    private void HandleSelection()
    {
        var selecteds = ItemList.GetSelectedItems();
        if (selecteds.Count() > 1) throw new Exception();
        if (selecteds.Count() == 0)
        {
            Value = default;
            return;
        }
        var selected = _items[selecteds[0]];
        Value = selected;
        _selectAction(selected);
        JustSelected?.Invoke(selected);
    }
    private void AddItemToList(T item)
    {
        ItemList.AddItem(_getLabelText(item),
            _getTexture is not null
                ? _getTexture(item)
                : null
        );
    }
    public void Add(T t)
    {
        _items.Add(t);
        AddItemToList(t);
    }
    public void Remove(T t)
    {
        var index = _items.IndexOf(t);
        var selecteds = ItemList.GetSelectedItems();
        if (selecteds.Count() > 1) throw new Exception();
        ItemList.RemoveItem(index);
        _items.Remove(t);
        HandleSelection();
    }

    public void AddOrReplace(Func<T, bool> pred, T replacement)
    {
        if (_items.Any(pred))
        {
            var toReplace = _items.FirstOrDefault(pred);
            Remove(toReplace);
        }
        
        Add(replacement);
    }

    public void SelectAt(int index)
    {
        ItemList.Select(index);
        HandleSelection();
    }

    public void Select(T t)
    {
        var index = _items.IndexOf(t);
        if (index == -1) return;
        SelectAt(index);
    }
}
