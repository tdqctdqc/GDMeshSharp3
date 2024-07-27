using System.Collections.Generic;
using Godot;

public partial class UnitTree : Tree
{
    private int _startColumn;

    public UnitTree(int startColumn)
    {
        _startColumn = startColumn;
        Columns = 3 + _startColumn;
        CreateItem();
    }

    public static void Add(Tree tree,
        Unit unit,
        int startColumn, Client c)
    {
        var i = tree.CreateItem(tree.GetRoot());
        Setup(i, unit, startColumn, c);
    }
    public static void Add(Tree tree,
        IEnumerable<Unit> units, 
        int startColumn,
        Client c)
    {
        var r = tree.GetRoot();
        foreach (var unit in units)
        {
            var item = tree.CreateItem(r);
            Setup(item, unit, startColumn, c);
        }
    }
    public static void Setup(
        TreeItem item,
        Unit unit, 
        int startColumn,
        Client c)
    {
        var d = c.Data;
        Setup(item, unit.Troops, 
            unit.GetMaxPowerTroop(d).Icon.Texture,
            $"{unit.Template.Get(d).Name} {unit.Id}",
            unit.Id, startColumn, c);
    }
    
    
    public static void Setup(
        TreeItem item,
        IdCount<Troop> troops, 
        Texture2D texture,
        string descr,
        int metadata,
        int startColumn,
        Client c)
    {
        item.SetMetadata(0, metadata);
        item.SetCellMode(0 + startColumn, TreeItem.TreeCellMode.Icon);
        item.SetIcon(0 + startColumn, texture);
        item.SetIconRegion(0 + startColumn, new Rect2(0f, 0f, 20f, 20f));
        item.SetCellMode(1 + startColumn, TreeItem.TreeCellMode.String);
        item.SetText(1 + startColumn, descr);
        var small = c.Settings.SmallIconSize.Value;
        var d = c.Data;
        foreach (var (troop, amt) in troops.GetEnumModel(d))
        {
            var iconSize = troop.Icon.Texture.GetSize();
            iconSize /= iconSize.Y;
            iconSize *= small;
            var troopBranch = item.CreateChild();
            troopBranch.SetMetadata(0, troop.Id);
            troopBranch.SetCellMode(1 + startColumn, TreeItem.TreeCellMode.Icon);
            troopBranch.SetIcon(1 + startColumn, troop.Icon.Texture);
            troopBranch.SetIconRegion(1 + startColumn, new Rect2(0f, 0f, iconSize.X, iconSize.Y));
            troopBranch.SetCellMode(2 + startColumn, TreeItem.TreeCellMode.String);
            troopBranch.SetText(2 + startColumn, 
                amt.ToString());
        }
    }

}