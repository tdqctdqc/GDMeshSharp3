using Godot;

public partial class ArmyTree : Tree
{
    private int _startColumn;

    public ArmyTree(Army a, 
        int startColumn,
        Data d)
    {
        _startColumn = startColumn;
        Columns = 4 + _startColumn;
        var root = CreateItem();
        Setup(root, a, _startColumn, d);
    }
    public static void Setup(
        TreeItem item,
        Army a, 
        int startColumn,
        Data d)
    {
        item.SetMetadata(0, a.Id);
        item.SetCellMode(0 + startColumn, TreeItem.TreeCellMode.Icon);
        item.SetIcon(0 + startColumn, a.Regime.Get(d).Template.Get(d).Flag.Texture);
        item.SetIconRegion(0 + startColumn, new Rect2(0f, 0f, 20f, 20f));
        item.SetCellMode(1 + startColumn, TreeItem.TreeCellMode.String);
        item.SetText(1 + startColumn, a.Id.ToString());
        foreach (var unit in a.Units.Entities(d))
        {
            AddUnit(item, unit, d);
        }
    }

    public Army GetArmy(Data d)
    {
        var id = GetRoot().GetMetadata(0).AsInt32();
        if (d.HasEntity(id) && d.Get<Entity>(id) is Army a) return a;
        return null;
    }
    public Unit GetSelectedUnit(Data d)
    {
        var selected = GetSelected();
        var metaData = selected.GetMetadata(0)
            .AsInt32();
        if (d.HasEntity(metaData)
            && d.Get<Entity>(metaData) is Unit u)
        {
            return u;
        }

        return null;
    }

    public void RemoveUnit(Unit u)
    {
        var unitItem = this.GetFirstChildWhere(
            t => t.GetMetadata(0 + _startColumn).AsInt32() == u.Id);
        unitItem?.Free();
    }

    public static void AddUnit(TreeItem parent,
        Unit unit, Data d)
    {
        var i = parent.CreateChild();
        UnitTree.Setup(i, unit, 1, d);
    }
    public void AddUnit(Unit unit, Data d)
    {
        var i = CreateItem(GetRoot());
        UnitTree.Setup(i, unit, 1, d);
    }
}