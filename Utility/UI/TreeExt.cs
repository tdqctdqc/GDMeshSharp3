
using System;
using System.Collections.Generic;
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
    public static void Remove<T>(this Tree tree, T u)
        where T : IIdentifiable
    {
        var unitItem = tree.GetFirstChildWhere(
            t => t.GetMetadata(0).AsInt32() == u.Id);
        unitItem?.Free();
    }
    public static List<T> GetSelectedEntities<T>
        (this Tree tree, Data d)
            where T : Entity
    {
        var res = new List<T>();
        var selected = tree.GetNextSelected(null);
        while (selected is not null)
        {
            if (selected.CastToEntity<T>(d) is T u)
            {
                res.Add(u);
            }

            selected = tree.GetNextSelected(selected);
        }

        return res;
    }
    public static List<T> GetSelectedModels<T>
        (this Tree tree, Data d)
        where T : class, IModel
    {
        var res = new List<T>();
        var selected = tree.GetNextSelected(null);
        while (selected is not null)
        {
            if (selected.CastToModel<T>(d) is T u)
            {
                res.Add(u);
            }

            selected = tree.GetNextSelected(selected);
        }

        return res;
    }
    
    public static T GetSelectedEntity<T>(this Tree tree, Data d)
        where T : Entity
    {
        return tree.GetSelected().CastToEntity<T>(d);
    }
    public static T GetSelectedModel<T>(this Tree tree, Data d)
        where T : class, IModel
    {
        return tree.GetSelected().CastToModel<T>(d);
    }

    public static (Unit u, Troop t)? GetSelectedTroopAndUnit(
        this Tree tree, Data d)
    {
        var t = tree.GetSelectedModel<Troop>(d);

        if (t is null)
        {
            return null;
        }

        var parent = tree.GetSelected().GetParent();
        if (parent is null) return null;
        
        if (parent.CastToEntity<Unit>(d) is Unit u)
        {
            return (u, t);
        }

        return null;
    }

    public static T CastToEntity<T>(this TreeItem item, Data d)
        where T : Entity
    {
        var metaData = item.GetMetadata(0)
            .AsInt32();
        if (d.HasEntity(metaData)
            && d.Get<Entity>(metaData) is T t)
        {
            return t;
        }

        return null;
    }
    
    public static T CastToModel<T>(this TreeItem item, Data d)
        where T : class, IModel
    {
        var metaData = item.GetMetadata(0)
            .AsInt32();
        if (d.HasEntity(metaData)
            && d.Models.ModelsById.TryGetValue(metaData, out var m)
            && m is T t)
        {
            return t;
        }

        return null;
    }
}