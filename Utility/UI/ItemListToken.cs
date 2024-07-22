
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata;
using Godot;

public class ItemListToken<T>
{
    public HashSet<T> Selected { get; private set; }
    public event Action JustSelected;
    public ItemList ItemList { get; private set; }
    public IReadOnlyList<T> Items => _items;
    private List<T> _items;
    private Func<T, Texture2D> _getTexture;
    private int _textureHeight;
    private Func<T, string> _getLabelText;
    private int _textureWidthRatio = 1;
    public ItemListToken(
        IEnumerable<T> items, 
        Func<T, string> getLabelText, 
        Func<T, Texture2D> getTexture,
        int textureHeight,
        bool multiSelect)
    {
        ItemList = new ItemList();
        ItemList.SelectMode = multiSelect
            ? ItemList.SelectModeEnum.Multi
            : ItemList.SelectModeEnum.Single;
        Selected = new HashSet<T>();
        ItemList.FixedIconSize = textureHeight * Vector2I.One;
        if (items.Any())
        {
            _items = items.ToList();
        }
        else
        {
            _items = new List<T>();
        }
        _getLabelText = getLabelText;
        _getTexture = getTexture;
        _textureHeight = textureHeight;
        
        SetList();
        ItemList.AllowReselect = true;
        ItemList.MultiSelected += (index, selected) =>
        {
            HandleSelection();
        };
        ItemList.ItemSelected += i =>
        {
            HandleSelection();
        };
    }
    
    
    
    public ItemListToken(
        IEnumerable<T> items, 
        Func<T, string> getLabelText, 
        bool multiSelect)
    {
        ItemList = new ItemList();
        ItemList.SelectMode = multiSelect
            ? ItemList.SelectModeEnum.Multi
            : ItemList.SelectModeEnum.Single;
        Selected = new HashSet<T>();
        _items = items.ToList();
        _getLabelText = getLabelText;
        SetList();
        ItemList.AllowReselect = true;
        ItemList.ItemSelected += i =>
        {
            HandleSelection();
        };
        ItemList.MultiSelected += (index, selected) =>
        {
            HandleSelection();
        };
    }

    public void Reset(IEnumerable<T> items, 
        bool keepSelection = true)
    {
        var values = Selected.Intersect(items).ToList();

        _items = items.ToList();
        ItemList.Clear();
        SetList();
        if (keepSelection)
        {
            Select(values);
        }
    }
    public void RefreshText()
    {
        for (var i = 0; i < _items.Count; i++)
        {
            ItemList.SetItemText(i, _getLabelText(_items[i]));
        }
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
        Selected.Clear();
        Selected.UnionWith(selecteds.Select(s => _items[s]));
        JustSelected?.Invoke();
    }
    private void AddItemToList(T item)
    {
        if (_getTexture is not null)
        {
            var texture = _getTexture(item);
            var i = ItemList.AddItem(_getLabelText(item),
                texture);
            var size = texture.GetSize();
            size /= size.Y;
            var w = (int)size.X;
            if (w > _textureWidthRatio)
            {
                _textureWidthRatio = w;
                ItemList.FixedIconSize = new Vector2I(w * _textureHeight,
                    _textureHeight);
            }
        }
        else
        {
            ItemList.AddItem(_getLabelText(item));
        }
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

    public void Select(IEnumerable<T> ts)
    {
        foreach (var t in ts)
        {
            var index = _items.IndexOf(t);
            ItemList.Select(index);
        }
        HandleSelection();
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
