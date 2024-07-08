using Godot;

public partial class ArmyTree : Tree
{
    private int _startColumn;

    public ArmyTree(Army a, 
        int startColumn,
        Client c)
    {
        _startColumn = startColumn;
        Columns = 4 + _startColumn;
        var root = CreateItem();
        Setup(root, a, _startColumn, c);
    }
    public static void Setup(
        TreeItem item,
        Army a, 
        int startColumn,
        Client c)
    {
        var d = c.Data;
        var small = c.Settings.SmallIconSize.Value;
        var flagSize = (Vector2)Regime.FlagAspectRatio;
        flagSize /= flagSize.Y;
        flagSize *= small;
        item.SetMetadata(0, a.Id);
        item.SetCellMode(0 + startColumn, TreeItem.TreeCellMode.Icon);
        item.SetIcon(0 + startColumn, a.Regime.Get(d).Template.Get(d).Flag.Texture);
        item.SetIconRegion(0 + startColumn, 
            new Rect2(0f, 0f, flagSize.X, flagSize.Y));
        item.SetCellMode(1 + startColumn, TreeItem.TreeCellMode.String);
        item.SetText(1 + startColumn, a.Id.ToString());
        foreach (var unit in a.Units.Entities(d))
        {
            AddUnit(item, unit, c);
        }
    }

    public Army GetArmy(Data d)
    {
        var id = GetRoot().GetMetadata(0).AsInt32();
        if (d.HasEntity(id) && d.Get<Entity>(id) is Army a) return a;
        return null;
    }

    

    public static void AddUnit(TreeItem parent,
        Unit unit, Client c)
    {
        var i = parent.CreateChild();
        UnitTree.Setup(i, unit, 1, c);
    }
    public void AddUnit(Unit unit, Client c)
    {
        var i = CreateItem(GetRoot());
        UnitTree.Setup(i, unit, 1, c);
    }
}