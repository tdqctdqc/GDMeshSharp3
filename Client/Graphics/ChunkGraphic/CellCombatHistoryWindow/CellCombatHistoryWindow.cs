
using Godot;
using Ui.CellCombatHistoryWindow;

public partial class CellCombatHistoryWindow : TabWindow
{
    public CellCombatHistory History { get; private set; }
    public CellCombatHistoryWindow(Client c)
        : base(c)
    {
        Size = new Vector2I(1000, 800);
        var general = new GeneralTab(this);
        AddTab(general);
    }

    public void Setup(CellCombatHistory history, Client client)
    {
        if (history == null) return;
        History = history;
    }
}