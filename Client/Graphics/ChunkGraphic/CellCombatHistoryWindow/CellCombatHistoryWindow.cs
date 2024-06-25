
using Godot;
using Ui.CellCombatHistoryWindow;

public partial class CellCombatHistoryWindow : TabWindow
{
    public CellDefenseNode Info { get; private set; }
    public CellCombatHistoryWindow(Client c)
        : base(c)
    {
        Size = new Vector2I(1000, 800);
        var general = new GeneralTab(this);
        AddTab(general);
        var units = new UnitsTab(this);
        AddTab(units);
    }

    public void Setup(CellDefenseNode info, Client client)
    {
        Info = info;
    }
}