using Godot;
namespace Ui.RegimeOverview;

public partial class BudgetTab : ScrollContainer, IUiDrawable
{
    private VBoxContainer _container;
    private RegimeOverviewWindow _parent;
    public BudgetTab(RegimeOverviewWindow parent)
    {
        _parent = parent;
        Name = "Budget";
        AnchorsPreset = (int)LayoutPreset.FullRect;
        _container = new VBoxContainer();
        AddChild(_container);
        _container.AnchorsPreset = (int)LayoutPreset.FullRect;
    }

    private BudgetTab()
    {
    }

    public void Draw(Client client)
    {
        _container.ClearChildren();
        var regime = _parent.Regime;
        if (regime is null) return;
        var ais = client.Data.HostLogicData.RegimeAis;
        if (ais.Dic.ContainsKey(regime) == false) return;
        var ai = ais[regime];
        var budget = ai.Budget;
        
    }
}