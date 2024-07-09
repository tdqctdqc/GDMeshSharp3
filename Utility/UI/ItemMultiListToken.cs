//
// using System;
// using System.Collections.Generic;
// using System.Linq;
// using Godot;
//
// public class ItemMultiListToken<T>
// {
//     public List<T> Selected { get; private set; }
//     public ItemList ItemList { get; private set; }
//     private List<T> _items;
//     private Func<T, Texture2D> _getTexture;
//     private Vector2I? _textureSize;
//     private Func<T, string> _getLabelText;
//     private Action<List<T>> _selectAction;
//     
//     public ItemMultiListToken(
//         IEnumerable<T> items, 
//         Func<T, string> getLabelText, 
//         Action<List<T>> selectAction,
//         Vector2 size,
//         Func<T, Texture2D> getTexture = null,
//         Vector2I? textureSize = null)
//     {
//         Selected = new List<T>();
//         ItemList = new ItemList();
//         ItemList.CustomMinimumSize = size;
//         ItemList.SelectMode = ItemList.SelectModeEnum.Multi;
//         if (textureSize.HasValue)
//         {
//             ItemList.FixedIconSize = textureSize.Value;
//         }
//
//         _selectAction = selectAction;
//         _getLabelText = getLabelText;
//         _getTexture = getTexture;
//         _textureSize = textureSize;
//         _items = new List<T>(items);
//         SetList();
//         ItemList.MultiSelected += (index, selected) =>
//         {
//             HandleMultiSelection();
//         };
//     }
//     public void RefreshText()
//     {
//         for (var i = 0; i < _items.Count; i++)
//         {
//             ItemList.SetItemText(i, _getLabelText(_items[i]));
//         }
//     }
//     public void Reset(IEnumerable<T> items)
//     {
//         _items = items.ToList();
//         ItemList.Clear();
//         SetList();
//     }
//     private void SetList()
//     {
//         foreach (var item in _items)
//         {
//             AddItemToList(item);
//         }
//     }
//
//     private void AddItemToList(T item)
//     {
//         ItemList.AddItem(_getLabelText(item),
//             _getTexture is not null
//                 ? _getTexture(item)
//                 : null
//         );
//     }
//
//     public void Add(T t)
//     {
//         _items.Add(t);
//         AddItemToList(t);
//     }
//     public void Remove(IEnumerable<T> toRemove)
//     {
//         foreach (var t in toRemove)
//         {
//             Selected.Remove(t);
//             _items.Remove(t);
//         }
//         ItemList.Clear();
//         SetList();
//         foreach (var t in Selected)
//         {
//             var index = _items.IndexOf(t);
//             ItemList.Select(index);
//         }
//         HandleMultiSelection();
//     }
//
//     private void HandleMultiSelection()
//     {
//         Selected = ItemList.GetSelectedItems()
//             .Select(i => _items[i]).ToList();
//         _selectAction(Selected);
//     }
// }