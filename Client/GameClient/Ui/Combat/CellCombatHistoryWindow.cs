
using System.Linq;
using Godot;
using Ui.Combat;

public partial class CellCombatHistoryWindow : TabWindow
{
    public CombatInfo Info { get; private set; }
    public CellCombatHistoryWindow(Client c)
        : base(c)
    {
        Size = new Vector2I(1000, 800);
        Info = new CombatInfo();
        var general = new GeneralTab(Info);
        AddTab(general);
        var units = new UnitsTab(Info);
        AddTab(units);
    }

    public void Setup(CellDefenseNode info, CombatGraph graph,
        Client client)
    {
        // Cell = info.Cell.Get(client.Data);
        Info.Setup(info, graph, client.Data);
    }

    public static void Open(Cell cell, CombatGraph graph, Client client)
    {
        var w = client.WindowManager.GetWindow<CellCombatHistoryWindow>();
        if (graph is not null
            && graph.CellDefNodes.TryGetValue(cell.MakeRef(), out var cellDefId))
        {
            var node = (CellDefenseNode)graph.NodesById[cellDefId];
            w.Setup(node, graph, client);
        }
        else
        {
            w.Setup(null, graph, client);
        }
        client.WindowManager.OpenWindow<CellCombatHistoryWindow>();
    }
}