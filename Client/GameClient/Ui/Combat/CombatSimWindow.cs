
using Godot;
using Ui.Combat;

public partial class CombatSimWindow : TabWindow
{
    public CombatInfo Info { get; private set; }
    public CombatSimWindow(Client c)
        : base(c)
    {
        Size = new Vector2I(1000, 800);
        Info = new CombatInfo();
        var sim = new SimTab(Info);
        AddTab(sim);
        var general = new GeneralTab(Info);
        AddTab(general);
        var units = new UnitsTab(Info);
        AddTab(units);
    }

    public void Setup(Client client)
    {
        Info.Setup(
            client.Data.Models.Landforms.Plain,
            client.Data.Models.Vegetations.Barren,
            new UnitCombatInfo[]{},
            new UnitCombatInfo[]{},
            false);
    }
}