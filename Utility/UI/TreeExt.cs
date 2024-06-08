
using System;
using Godot;

public static class TreeExt
{
    public static TreeItem GetFirstChildWhere(this Tree t,
        Func<TreeItem, bool> pred)
    {
        var curr = t.GetRoot();
        while (curr is not null)
        {
            if (pred(curr)) return curr;
            curr = curr.GetNextInTree();
        }

        return null;
    }
}