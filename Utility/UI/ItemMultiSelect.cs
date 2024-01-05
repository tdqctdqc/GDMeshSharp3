using System;
using System.Collections.Generic;
using System.Linq;
using Godot;

public partial class ItemMultiSelect : ItemList
{
    public Dictionary<int, object> Items { get; private set; }
    public static ItemMultiSelect ConstructText<T>(List<T> elements, Func<T, string> getName,
        Action selectedAction, Func<T, Color> getColor = null)
    {
        return Construct(elements, (t, l) => l.AddItem(getName(t)),
            selectedAction, getColor);
    }
    public static ItemMultiSelect ConstructIcon<T>(List<T> elements, 
        Func<T, Icon> getIcon, 
        float height,
        Action selectedAction, Func<T, Color> getColor = null)
    {
        var size = getIcon(elements.First()).GetDim(height);
        var l = Construct(elements, (t, l) =>
            {
                l.AddIconItem(getIcon(t).Texture);
            },
            selectedAction, getColor);
        l.FixedIconSize = (Vector2I)size;
        return l;
    }

    private static ItemMultiSelect Construct<T>(List<T> elements, Action<T, ItemMultiSelect> add, 
        Action selectedAction, Func<T, Color> getColor = null)
    {
        var list = new ItemMultiSelect();
        for (var i = 0; i < elements.Count; i++)
        {
            var element = elements[i];
            list.Items.Add(i, element);
            add(element, list);
            if(getColor != null) list.SetItemCustomBgColor(i, getColor(element));
        }
        list.MultiSelected += (l, b) => selectedAction();
        return list;
    }
    private ItemMultiSelect()
    {
        AllowReselect = true;
        AutoHeight = true;
        FocusMode = FocusModeEnum.None;
        Items = new Dictionary<int, object>();
        SelectMode = SelectModeEnum.Multi;
    }

    public IEnumerable<T> GetSelectedItems<T>()
    {
        return GetSelectedItems().Select(i => (T) Items[i]);
    }
}
