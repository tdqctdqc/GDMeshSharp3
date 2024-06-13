using Godot;

public partial class ArmyTree : Tree
{

    public void Setup(Army a, Data d)
    {
        this.Clear();
        Columns = 4;
        var root = CreateItem();
        root.SetMetadata(0, a.Id);
        root.SetCellMode(0, TreeItem.TreeCellMode.Icon);
        root.SetIcon(0, a.Regime.Get(d).Template.Get(d).Flag.Texture);
        root.SetIconRegion(0, new Rect2(0f, 0f, 20f, 20f));

        root.SetCellMode(1, TreeItem.TreeCellMode.String);
        root.SetText(1, a.Id.ToString());
        foreach (var unit in a.Units.Entities(d))
        {
            AddUnit(unit, d);
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
            t => t.GetMetadata(0).AsInt32() == u.Id);
        unitItem?.Free();
    }

    public void AddUnit(Unit unit, Data d)
    {
        var unitBranch = CreateItem(GetRoot());
        unitBranch.SetMetadata(0, unit.Id);
        unitBranch.SetCellMode(1, TreeItem.TreeCellMode.Icon);
        unitBranch.SetIcon(1, unit.GetMaxPowerTroop(d).Icon.Texture);
        unitBranch.SetIconRegion(1, new Rect2(0f, 0f, 20f, 20f));

        unitBranch.SetCellMode(2, TreeItem.TreeCellMode.String);
        unitBranch.SetText(2, $"{unit.Template.Get(d).Name} {unit.Id}");
        foreach (var (troop, amt) in unit.Troops.GetEnumerableModel(d))
        {
            var troopBranch = unitBranch.CreateChild();
            troopBranch.SetCellMode(2, TreeItem.TreeCellMode.Icon);
            troopBranch.SetIcon(2, troop.Icon.Texture);
            troopBranch.SetIconRegion(2, new Rect2(0f, 0f, 20f, 20f));
            troopBranch.SetCellMode(3, TreeItem.TreeCellMode.String);
            troopBranch.SetText(3, $"{amt} / {unit.Template.Get(d).TroopCounts.Get(troop)}");
        }
    }
}