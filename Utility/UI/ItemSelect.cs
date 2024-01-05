using System;
using System.Collections.Generic;
using System.Linq;
using Godot;

public partial class ItemSelect : ItemList
{
    public object Selected { get; private set; }
    public Dictionary<int, object> Items { get; private set; }
    public static ItemSelect ConstructText<T>(List<T> elements, Func<T, string> getName,
        Action<T> selectedAction, Func<T, Color> getColor = null)
    {
        return Construct(elements, (t, l) => l.AddItem(getName(t)),
            selectedAction, getColor);
    }
    public static ItemSelect ConstructIcon<T>(List<T> elements, 
        Func<T, Icon> getIcon, float height,
        Action<T> selectedAction, Func<T, Color> getColor = null)
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

    private static ItemSelect Construct<T>(List<T> elements, Action<T, ItemSelect> add, 
        Action<T> selectedAction, Func<T, Color> getColor = null)
    {
        var list = new ItemSelect();
        for (var i = 0; i < elements.Count; i++)
        {
            var element = elements[i];
            list.Items.Add(i, element);
            add(element, list);
            if(getColor != null) list.SetItemCustomBgColor(i, getColor(element));
        }
        list.ItemSelected += l =>
        {
            var i = (T) list.Items[(int) l];
            list.Selected = i;
            selectedAction(i);
        };
        return list;
    }
    private ItemSelect()
    {
        AutoHeight = true;
        FocusMode = FocusModeEnum.None;
        Items = new Dictionary<int, object>();
        SelectMode = SelectModeEnum.Single;
    }
}
