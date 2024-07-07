using Godot;

public partial class UnitTree : Tree
{
    private int _startColumn;

    public UnitTree(Unit unit, 
        int startColumn,
        Data d)
    {
        var i = CreateItem(GetRoot());
        _startColumn = startColumn;
        Columns = 3 + _startColumn;

        Setup(i, unit, startColumn, d);
    }
    public static void Setup(
        TreeItem item,
        Unit unit, 
        int startColumn,
        Data d)
    {
        Setup(item, unit.Troops, unit.Template.Get(d).Troops,
            unit.GetMaxPowerTroop(d).Icon.Texture,
            $"{unit.Template.Get(d).Name} {unit.Id}",
            unit.Id, startColumn, d);
    }
    
    public static void Setup(
        TreeItem item,
        UnitTemplate template, 
        int startColumn,
        Data d)
    {
        Setup(item, template.Troops,
            template.Troops,
            template.GetMaxPowerTroop(d).Icon.Texture,
            $"{template.Name}",
            template.Id, startColumn, d);
    }
    
    public static void Setup(
        TreeItem item,
        IdCount<Troop> troops, 
        IdCount<Troop> troopsIdeal, 
        Texture2D texture,
        string descr,
        int metadata,
        int startColumn,
        Data d)
    {
        item.SetMetadata(0, metadata);
        item.SetCellMode(0 + startColumn, TreeItem.TreeCellMode.Icon);
        item.SetIcon(0 + startColumn, texture);
        item.SetIconRegion(0 + startColumn, new Rect2(0f, 0f, 20f, 20f));
        item.SetCellMode(1 + startColumn, TreeItem.TreeCellMode.String);
        item.SetText(1 + startColumn, descr);
        foreach (var (troop, amt) in troops.GetEnumModel(d))
        {
            var troopBranch = item.CreateChild();
            troopBranch.SetMetadata(0, troop.Id);
            troopBranch.SetCellMode(1 + startColumn, TreeItem.TreeCellMode.Icon);
            troopBranch.SetIcon(1 + startColumn, troop.Icon.Texture);
            troopBranch.SetIconRegion(1 + startColumn, new Rect2(0f, 0f, 20f, 20f));
            troopBranch.SetCellMode(2 + startColumn, TreeItem.TreeCellMode.String);
            troopBranch.SetText(2 + startColumn, 
                troops == troopsIdeal
                ? amt.ToString()
                : $"{amt} / {troopsIdeal.Get(troop)}");
        }
    }
    
    
    public Troop GetSelectedTroop(Data d)
    {
        var selected = GetSelected();
        var metaData = selected.GetMetadata(0)
            .AsInt32();
        if (d.Models.ModelsById.TryGetValue(metaData, out var m)
            && m is Troop t)
        {
            return t;
        }

        return null;
    }
}